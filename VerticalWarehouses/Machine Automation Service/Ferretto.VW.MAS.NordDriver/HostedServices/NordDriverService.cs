using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.EnIPStack;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.InverterDriver;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.NordDriver
{
    internal partial class NordDriverService : AutomationBackgroundService<FieldCommandMessage, FieldNotificationMessage, FieldCommandEvent, FieldNotificationEvent>
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly Task explicitMessagesTask;

        private readonly BlockingConcurrentQueue<InverterMessage> inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

        private readonly ISocketTransport socketTransport;

        private IPAddress inverterAddress;

        //private readonly Dictionary<InverterIndex, IInverterStateMachine> currentStateMachines = new Dictionary<InverterIndex, IInverterStateMachine>();
        private bool isDisposed;

        private InverterMessage processData;

        private int sendPort;

        #endregion

        #region Constructors

        public NordDriverService(
            ILogger<NordDriverService> logger,
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory,
            ISocketTransport socketTransport)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.socketTransport = socketTransport ?? throw new ArgumentNullException(nameof(socketTransport));

            this.explicitMessagesTask = new Task(async () => await this.ExplicitMessages());
        }

        #endregion

        #region Methods

        public void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                this.inverterCommandQueue?.Dispose();
                return;
            }

            if (disposing)
            {
            }

            this.isDisposed = true;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            await base.StartAsync(cancellationToken);
        }

        protected override bool FilterCommand(FieldCommandMessage command)
        {
            return
                command.Destination == FieldMessageActor.InverterDriver
                ||
                command.Destination == FieldMessageActor.Any;
        }

        protected override bool FilterNotification(FieldNotificationMessage notification)
        {
            return
                notification.Destination == FieldMessageActor.InverterDriver
                ||
                notification.Destination == FieldMessageActor.Any;
        }

        protected override Task OnCommandReceivedAsync(FieldCommandMessage receivedMessage, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace($"1:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}");

            var inverterIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            // TODO implement commands

            return Task.CompletedTask;
        }

        protected override async Task OnNotificationReceivedAsync(FieldNotificationMessage receivedMessage, IServiceProvider serviceProvider)
        {
            var inverterIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());
            switch (receivedMessage.Type)
            {
                case FieldMessageType.DataLayerReady:
                    // start communication
                    await this.StartHardwareCommunicationsAsync(serviceProvider);
                    //this.InitializeTimers(serviceProvider);
                    break;

                    // TODO implement close of state machines
            }

            if (receivedMessage.Source == FieldMessageActor.InverterDriver
                && (receivedMessage.Status == MessageStatus.OperationEnd
                    || receivedMessage.Status == MessageStatus.OperationStop
                    )
                )
            {
                var notificationMessageToFsm = receivedMessage;

                // forward the message to upper level
                notificationMessageToFsm.Destination = FieldMessageActor.DeviceManager;

                this.eventAggregator?
                    .GetEvent<FieldNotificationEvent>()
                    .Publish(notificationMessageToFsm);
            }
        }

        private async Task ExplicitMessages()
        {
            do
            {
                try
                {
                    if (this.socketTransport.IsConnected
                        && this.inverterCommandQueue.TryPeek(Timeout.Infinite, this.CancellationToken, out var inverterMessage)
                        && inverterMessage != null)
                    {
                        this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");
                        this.Logger.LogTrace($"2:Command queue length: {this.inverterCommandQueue.Count}");

                        var result = await this.ProcessInverterCommand(inverterMessage);

                        if (result)
                        {
                            this.inverterCommandQueue.Dequeue(out _);
                        }
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
                {
                    this.Logger.LogDebug("Terminating ExplicitMessages task.");
                    break;
                }
            }
            while (!this.CancellationToken.IsCancellationRequested);
        }

        private void OnInverterMessageReceivedExplicit(byte[] messageBytes)
        {
            this.Logger.LogTrace($"1:inverterMessage={messageBytes}");
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                try
                {
                    var message = InverterMessage.FromBytesExplicit(messageBytes);

                    //this.currentStateMachines.TryGetValue(message.SystemIndex, out var messageCurrentStateMachine);

                    if (message.IsError)
                    {
                        this.Logger.LogError($"Received error Message: {message}");
                        var errorCode = (int)DataModels.MachineErrorCode.InverterErrorBaseCode + message.UShortPayload;
                        if (!Enum.IsDefined(typeof(DataModels.MachineErrorCode), errorCode))
                        {
                            errorCode = (int)DataModels.MachineErrorCode.InverterErrorBaseCode;
                        }

                        scope.ServiceProvider
                            .GetRequiredService<IErrorsProvider>()
                            .RecordNew((DataModels.MachineErrorCode)errorCode, additionalText: message.SystemIndex.ToString());
                    }

                    //if (message.IsWriteMessage)
                    //{
                    //    this.EvaluateWriteMessage(message, messageCurrentStateMachine, serviceProvider);
                    //}
                    //else
                    //{
                    //    this.EvaluateReadMessage(message, messageCurrentStateMachine, serviceProvider);
                    //}
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Exception while parsing Inverter raw message bytes {BitConverter.ToString(messageBytes)}");
                    scope.ServiceProvider.GetRequiredService<IErrorsProvider>().RecordNew(DataModels.MachineErrorCode.InverterConnectionError, BayNumber.BayOne, ex.Message);

                    this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, $"Exception {ex.Message} while parsing Inverter raw message bytes", 0), FieldMessageType.InverterException);
                }
            }
        }

        private void OnInverterMessageReceivedImplicit(object sender, ImplicitReceivedEventArgs e)
        {
            this.Logger.LogTrace($"1:inverterMessage={e.receivedMessage}");
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                try
                {
                    var message = InverterMessage.FromBytesImplicit(e.receivedMessage);
                    if (this.processData is null)
                    {
                        var invertersProvider = scope.ServiceProvider.GetRequiredService<INordProvider>();
                        this.processData = new InverterMessage(0, InverterParameterId.ControlWord, 0);
                        foreach (var inverter in invertersProvider.GetAll())
                        {
                            this.processData.SetPoint(inverter.SystemIndex, inverter.CommonControlWord.Value, inverter.SetPointFrequency, inverter.SetPointPosition, inverter.SetPointRampTime);
                        }
                        this.socketTransport.ImplicitMessageStart(this.processData.RawData);
                    }
                    //this.currentStateMachines.TryGetValue(message.SystemIndex, out var messageCurrentStateMachine);

                    if (message.IsError)
                    {
                        this.Logger.LogError($"Received error Message: {message}");
                        var errorCode = (int)DataModels.MachineErrorCode.InverterErrorBaseCode + message.UShortPayload;
                        if (!Enum.IsDefined(typeof(DataModels.MachineErrorCode), errorCode))
                        {
                            errorCode = (int)DataModels.MachineErrorCode.InverterErrorBaseCode;
                        }

                        scope.ServiceProvider
                            .GetRequiredService<IErrorsProvider>()
                            .RecordNew((DataModels.MachineErrorCode)errorCode, additionalText: message.SystemIndex.ToString());
                    }

                    //    this.EvaluateReadMessage(message, messageCurrentStateMachine, serviceProvider);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Exception while parsing Inverter raw message bytes {BitConverter.ToString(e.receivedMessage)}");
                    scope.ServiceProvider.GetRequiredService<IErrorsProvider>().RecordNew(DataModels.MachineErrorCode.InverterConnectionError, BayNumber.BayOne, ex.Message);

                    this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, $"Exception {ex.Message} while parsing Inverter raw message bytes", 0), FieldMessageType.InverterException);
                }
            }
        }

        private async Task<bool> ProcessInverterCommand(InverterMessage inverterMessage)
        {
            var serviceId = inverterMessage.IsWriteMessage ? CIPServiceCodes.SetAttributeSingle : CIPServiceCodes.GetAttributeSingle;
            var msg = inverterMessage.ToBytes();
            var datalen = msg.Length - 6;
            var data = new List<byte>();
            if (datalen > 0)
            {
                data.AddRange(msg.ToArray().Skip(6));
            }

            //TODO - instanceId is the parameter subindex
            var result = this.socketTransport.ExplicitMessage(101, 0, (ushort)inverterMessage.ParameterId, serviceId, data?.ToArray(), out var received);
            if (result)
            {
                this.OnInverterMessageReceivedExplicit(received);
            }
            return result;
        }

        private void SendOperationErrorMessage(InverterIndex inverterIndex, IFieldMessageData messageData, FieldMessageType type)
        {
            switch (type)
            {
                case FieldMessageType.InverterError:
                    var errorMsg = new FieldNotificationMessage(
                        messageData,
                        "Inverter Driver Error",
                       FieldMessageActor.Any,
                       FieldMessageActor.InverterDriver,
                       FieldMessageType.InverterError,
                       MessageStatus.OperationError,
                        (byte)inverterIndex,
                       ErrorLevel.Error);
                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(errorMsg);
                    break;

                case FieldMessageType.InverterException:
                    var exceptionMsg = new FieldNotificationMessage(
                     messageData,
                     "Inverter Driver Exception",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterException,
                    MessageStatus.OperationError,
                     (byte)inverterIndex,
                    ErrorLevel.Error);
                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(exceptionMsg);
                    break;

                case FieldMessageType.CalibrateAxis:
                    if (messageData is InverterExceptionFieldMessageData calibrateData)
                    {
                        var calibrateErrorNotification = new FieldNotificationMessage(
                        calibrateData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.CalibrateAxis,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(calibrateErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterSwitchOff:
                    if (messageData is InverterExceptionFieldMessageData switchOffData)
                    {
                        var inverterSwitchOffErrorNotification = new FieldNotificationMessage(
                        switchOffData,
                        $"Inverter status not configured for requested inverter {inverterIndex}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSwitchOff,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterSwitchOffErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterSwitchOn:
                    if (messageData is InverterExceptionFieldMessageData switchOnData)
                    {
                        var inverterSwitchOnErrorNotification = new FieldNotificationMessage(
                        switchOnData,
                        $"Inverter status not configured for requested inverter {inverterIndex}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSwitchOn,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterSwitchOnErrorNotification);
                    }
                    break;

                case FieldMessageType.Positioning:

                    if (messageData is InverterExceptionFieldMessageData positioningData)
                    {
                        var positioningErrorNotification = new FieldNotificationMessage(
                        null,
                        positioningData.ExceptionDescription,
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.Positioning,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(positioningErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterPowerOff:
                    if (messageData is InverterExceptionFieldMessageData powerOffData)
                    {
                        var inverterPowerOfferrorNotification = new FieldNotificationMessage(
                        powerOffData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOff,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterPowerOfferrorNotification);
                    }
                    break;

                case FieldMessageType.InverterPowerOn:
                    if (messageData is InverterExceptionFieldMessageData powerOnData)
                    {
                        var inverterPowerOnerrorNotification = new FieldNotificationMessage(
                        powerOnData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOn,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterPowerOnerrorNotification);
                    }
                    break;

                case FieldMessageType.ShutterPositioning:
                    if (messageData is InverterExceptionFieldMessageData shutterPositioningData)
                    {
                        var shutterPositioningErrorNotification = new FieldNotificationMessage(
                        shutterPositioningData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.ShutterPositioning,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(shutterPositioningErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterStop:
                    var inverterStopErrorNotification = new FieldNotificationMessage(
                   null,
                   $"Inverter status not configured for requested inverter {inverterIndex}",
                   FieldMessageActor.Any,
                   FieldMessageActor.InverterDriver,
                   FieldMessageType.InverterStop,
                   MessageStatus.OperationError,
                   (byte)inverterIndex,
                   ErrorLevel.Error);
                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterStopErrorNotification);
                    break;

                case FieldMessageType.InverterStatusUpdate:
                    if (messageData is InverterExceptionFieldMessageData updateData)
                    {
                        var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                        updateData,
                        "Wrong message Data data type",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterStatusUpdate,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterSetTimer:
                    if (messageData is InverterExceptionFieldMessageData setTimerData)
                    {
                        var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                        setTimerData,
                        "Wrong message Data data type",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSetTimer,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterReading:
                    if (messageData is InverterExceptionFieldMessageData inverterReadingData)
                    {
                        var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                        inverterReadingData,
                        "Error during reading inverter parameter",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterReading,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterProgramming:
                    if (messageData is InverterExceptionFieldMessageData inverterProgrammingData)
                    {
                        var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                        inverterProgrammingData,
                        "Error during writing inverter parameter",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterProgramming,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;
            }
        }

        private Task StartHardwareCommunicationsAsync(IServiceProvider serviceProvider)
        {
            var masterInverter = serviceProvider
                .GetRequiredService<IDigitalDevicesDataProvider>()
                .GetInverterByIndex(InverterIndex.MainInverter);

            this.inverterAddress = masterInverter.IpAddress;
            this.sendPort = masterInverter.TcpPort;

            try
            {
                // TEST
                var localAddress = new IPAddress(new byte[] { 192, 168, 250, 199 });
                this.inverterAddress = new IPAddress(new byte[] { 192, 168, 250, 53 });
                // END TEST
                this.socketTransport.Configure(this.inverterAddress, this.sendPort, localAddress);
                this.socketTransport.ImplicitReceivedChanged += this.OnInverterMessageReceivedImplicit;

                this.explicitMessagesTask.Start();
            }
            catch (Exception ex)
            {
                this.Logger.LogCritical($"Error while starting inverter socket threads: {ex.Message}");

                this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "while starting service threads", 0), FieldMessageType.InverterException);
                throw new InverterDriverException($"Exception {ex.Message} StartHardwareCommunications Failed 2", ex);
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
