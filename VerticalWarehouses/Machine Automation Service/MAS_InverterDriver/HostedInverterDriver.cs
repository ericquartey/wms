using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.MAS_InverterDriver.StateMachines.Stop;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember

namespace Ferretto.VW.MAS_InverterDriver
{
    public class HostedInverterDriver : BackgroundService
    {
        #region Fields

        private const int HEARTBEAT_TIMEOUT = 300;

        private readonly BlockingConcurrentQueue<FieldCommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IDataLayerValueManagment dataLayerValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<InverterMessage> heartbeatQueue;

        private readonly BlockingConcurrentQueue<InverterMessage> inverterCommandQueue;

        private readonly Task inverterReceiveTask;

        private readonly Task inverterSendTask;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly ISocketTransport socketTransport;

        private Timer controlWordCheckTimer;

        private IInverterStateMachine currentStateMachine;

        private bool disposed;

        private bool heartbeatCheck;

        private bool heartbeatSet;

        private Timer heartBeatTimer;

        private InverterMessage lastControlMessage;

        private InverterMessage lastHeartbeatMessage;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public HostedInverterDriver(IEventAggregator eventAggregator, ISocketTransport socketTransport, IDataLayerValueManagment dataLayerValueManagement, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.socketTransport = socketTransport;
            this.eventAggregator = eventAggregator;
            this.dataLayerValueManagement = dataLayerValueManagement;
            this.logger = logger;

            this.heartbeatQueue = new BlockingConcurrentQueue<InverterMessage>();
            this.inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

            this.commandQueue = new BlockingConcurrentQueue<FieldCommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

            this.commandReceiveTask = new Task(this.CommandReceiveTaskFunction);
            this.notificationReceiveTask = new Task(async () => await this.NotificationReceiveTaskFunction());
            this.inverterReceiveTask = new Task(async () => await this.ReceiveInverterData());
            this.inverterSendTask = new Task(async () => await this.SendInverterCommand());

            this.lastControlMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam);

            this.logger.LogTrace("2:Subscription Command");

            this.InitializeMethodSubscriptions();

            logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~HostedInverterDriver()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.heartBeatTimer?.Dispose();
                this.controlWordCheckTimer?.Dispose();
            }

            this.disposed = true;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogDebug("1:Method Start");

            this.stoppingToken = stoppingToken;

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception: {ex.Message} while starting service threads");

                throw new InverterDriverException($"Exception: {ex.Message} while starting service threads", ex);
            }

            this.logger.LogDebug("3:Method End");

            return Task.CompletedTask;
        }

        private void CommandReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            this.controlWordCheckTimer?.Dispose();
            this.controlWordCheckTimer = new Timer(this.ControlWordCheckTimeout, null, -1, Timeout.Infinite);

            this.heartBeatTimer?.Dispose();
            this.heartBeatTimer = new Timer(this.SendHeartBeat, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(HEARTBEAT_TIMEOUT));

            do
            {
                FieldCommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"2:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End operation cancelled");

                    return;
                }

                if (this.currentStateMachine != null && receivedMessage.Type != FieldMessageType.Stop)
                {
                    var errorNotification = new FieldNotificationMessage(null, "Inverter operation already in progress", FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver, receivedMessage.Type, MessageStatus.OperationError, ErrorLevel.Error);

                    this.logger.LogTrace($"4:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                    continue;
                }

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.CalibrateAxis:
                        if (receivedMessage.Data is ICalibrateAxisFieldMessageData calibrateData)
                        {
                            this.logger.LogDebug("5:Object creation");

                            this.currentStateMachine = new CalibrateAxisStateMachine(calibrateData.AxisToCalibrate, this.inverterCommandQueue, this.eventAggregator, this.logger);
                        }

                        break;

                    case FieldMessageType.InverterReset:
                        if (receivedMessage.Data is IResetInverterFieldMessageData stopData)
                        {
                            this.logger.LogDebug($"6:Condition={this.currentStateMachine == null}");

                            if (this.currentStateMachine == null)
                            {
                                // The state machine for Stop operation is invoked
                                this.currentStateMachine = new StopStateMachine(stopData.AxisToStop, this.inverterCommandQueue, this.eventAggregator, this.logger);
                            }
                            else
                            {
                                // Force a stop
                                this.currentStateMachine.Stop();
                            }
                        }
                        break;
                }
                this.currentStateMachine?.Start();
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("7:Method End");
        }

        private void ControlWordCheckTimeout(object state)
        {
            this.controlWordCheckTimer.Change(-1, Timeout.Infinite);
            InverterOperationTimeoutFieldMessageData notificationData = new InverterOperationTimeoutFieldMessageData(this.lastControlMessage.UShortPayload);
            var errorNotification = new FieldNotificationMessage(notificationData, "Control Word set timeout", FieldMessageActor.Any,
                FieldMessageActor.InverterDriver, FieldMessageType.InverterOperationTimeout, MessageStatus.OperationError, ErrorLevel.Error);

            this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
        }

        private void InitializeMethodSubscriptions()
        {
            var commandEvent = this.eventAggregator.GetEvent<FieldCommandEvent>();
            commandEvent.Subscribe(message =>
                {
                    this.commandQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == FieldMessageActor.InverterDriver || message.Destination == FieldMessageActor.Any);

            var notificationEvent = this.eventAggregator.GetEvent<FieldNotificationEvent>();
            notificationEvent.Subscribe(message =>
                {
                    this.notificationQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == FieldMessageActor.InverterDriver || message.Destination == FieldMessageActor.Any);
        }

        private async Task NotificationReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            do
            {
                FieldNotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"2:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End operation cancelled");

                    return;
                }

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.DataLayerReady:
                        await this.StartHardwareCommunications();
                        break;

                    case FieldMessageType.CalibrateAxis:
                    case FieldMessageType.Stop:
                    case FieldMessageType.InverterReset:
                        if (receivedMessage.Status == MessageStatus.OperationEnd || receivedMessage.Status == MessageStatus.OperationStop)
                        {
                            this.currentStateMachine?.Dispose();
                            this.currentStateMachine = null;
                        }
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("4:Method End");
        }

        private async Task ProcessCommand()
        {
            this.logger.LogDebug("1:Method Start");

            while (this.inverterCommandQueue.Dequeue(out var message))
            {
                this.logger.LogTrace($"2:ParameterId={message.ParameterId}:IsWriteMessage={message.IsWriteMessage}:SendDelay{message.SendDelay}");

                if (message.ParameterId == InverterParameterId.ControlWordParam)
                {
                    this.lastControlMessage = new InverterMessage(message);
                }

                var inverterMessagePacket = message.IsWriteMessage ? message.GetWriteMessage() : message.GetReadMessage();
                if (message.SendDelay > 0)
                {
                    await this.socketTransport.WriteAsync(inverterMessagePacket, message.SendDelay, this.stoppingToken);
                }
                else
                {
                    await this.socketTransport.WriteAsync(inverterMessagePacket, this.stoppingToken);
                }
            }

            this.logger.LogDebug("3:Method End");
        }

        private async Task ProcessHeartbeat()
        {
            this.logger.LogDebug("1:Method Start");

            while (this.heartbeatQueue.Dequeue(out var message))
            {
                this.logger.LogTrace($"2:message={message}");

                await this.socketTransport.WriteAsync(message.GetHeartbeatMessage(this.heartbeatSet), this.stoppingToken);

                this.lastHeartbeatMessage = message;

                this.heartbeatSet = !this.heartbeatSet;
                this.heartbeatCheck = false;
            }

            this.logger.LogDebug("3:Method End");
        }

        private async Task ReceiveInverterData()
        {
            this.logger.LogDebug("1:Method Start");

            do
            {
                byte[] inverterData;
                try
                {
                    inverterData = await this.socketTransport.ReadAsync(this.stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End operation cancelled");

                    return;
                }

                this.logger.LogTrace($"3:inverterData[1]={inverterData[1]}");

                //INFO: Byte 1 of read data contains packet length, zero means invalid packet
                if (inverterData[1] == 0x00)
                {
                    continue;
                }

                InverterMessage currentMessage;
                try
                {
                    currentMessage = new InverterMessage(inverterData);

                    this.logger.LogTrace($"4:currentMessage={currentMessage}");
                }
                catch (InverterDriverException)
                {
                    continue;
                }
                //TODO catch other exceptions

                if (currentMessage.IsWriteMessage && currentMessage.ParameterId == InverterParameterId.ControlWordParam)
                {
                    this.logger.LogTrace($"5:heartbeatCheck={this.heartbeatCheck}");

                    if (!this.heartbeatCheck)
                    {
                        this.logger.LogTrace($"6:currentMessage.UShortPayload={currentMessage.UShortPayload}");

                        if (currentMessage.UShortPayload == this.lastHeartbeatMessage.UShortPayload)
                        {
                            this.heartbeatCheck = true;
                            continue;
                        }
                    }
                    else
                    {
                        var readStatusWordMessage = new InverterMessage(0x00, (short)InverterParameterId.StatusWordParam);
                        this.inverterCommandQueue.Enqueue(readStatusWordMessage);

                        this.logger.LogTrace($"7:readStatusWordMessage={readStatusWordMessage}");

                        this.controlWordCheckTimer.Change(5000, Timeout.Infinite);
                        continue;
                    }
                }

                if (this.currentStateMachine != null)
                {
                    this.logger.LogTrace($"8:currentMessage={currentMessage}");

                    if (this.currentStateMachine.ProcessMessage(currentMessage))
                    {
                        try
                        {
                            this.controlWordCheckTimer.Change(-1, Timeout.Infinite);
                        }
                        catch (Exception)
                        {
                            this.logger.LogDebug("9:Method Exception");

                            this.controlWordCheckTimer = new Timer(this.ControlWordCheckTimeout, null, -1, Timeout.Infinite);
                        }
                    }
                    else
                    {
                        var readStatusWordMessage = new InverterMessage(0x00, (short)InverterParameterId.StatusWordParam);

                        this.logger.LogTrace($"10:readStatusWordMessage={readStatusWordMessage}");

                        this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                    }
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("11:Method End");
        }

        private void SendHeartBeat(object state)
        {
            this.heartbeatQueue.Enqueue(this.lastControlMessage);
        }

        private async Task SendInverterCommand()
        {
            this.logger.LogDebug("1:Method Start");

            //INFO Create WaitHandle array to wait for multiple events
            var commandHandles = new[]
            {
                this.heartbeatQueue.WaitHandle,
                this.inverterCommandQueue.WaitHandle
            };

            do
            {
                var handleIndex = WaitHandle.WaitAny(commandHandles);

                this.logger.LogTrace($"2:handleIndex={handleIndex}");

                switch (handleIndex)
                {
                    case 0:
                        await this.ProcessHeartbeat();
                        break;

                    case 1:
                        await this.ProcessCommand();
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("3:Method End");
        }

        private async Task StartHardwareCommunications()
        {
            this.logger.LogDebug("1:Method Start");

            var inverterAddress = await
                this.dataLayerValueManagement.GetIPAddressConfigurationValueAsync((long)SetupNetwork.Inverter1, (long)ConfigurationCategory.SetupNetwork);
            var inverterPort = await this.dataLayerValueManagement.GetIntegerConfigurationValueAsync((long)SetupNetwork.Inverter1Port, (long)ConfigurationCategory.SetupNetwork);

            this.socketTransport.Configure(inverterAddress, inverterPort);

            try
            {
                await this.socketTransport.ConnectAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception {ex.Message} while Connecting Receiver Socket Transport");

                throw new InverterDriverException($"Exception {ex.Message} while Connecting Receiver Socket Transport", ex);
            }

            if (!this.socketTransport.IsConnected)
            {
                this.logger.LogCritical("3:Socket Transport failed to connect");

                throw new InverterDriverException("Socket Transport failed to connect");
            }

            try
            {
                this.inverterReceiveTask.Start();
                this.inverterSendTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"4:Exception: {ex.Message} while starting service threads");

                throw new InverterDriverException($"Exception: {ex.Message} while starting service threads", ex);
            }

            this.logger.LogDebug("5:Method End");
        }

        #endregion
    }
}
