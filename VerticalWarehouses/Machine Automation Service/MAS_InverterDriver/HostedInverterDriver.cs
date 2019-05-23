using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning;
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
    public partial class HostedInverterDriver : BackgroundService
    {
        #region Fields

        private const int AXIS_POSITION_UPDATE_INTERVAL = 25;

        private const int HEARTBEAT_TIMEOUT = 9000;   // 300

        private const int SENSOR_STATUS_UPDATE_INTERVAL = 50000;

        private readonly BlockingConcurrentQueue<FieldCommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<InverterMessage> heartbeatQueue;

        private readonly BlockingConcurrentQueue<InverterMessage> inverterCommandQueue;

        private readonly InverterIoStatus inverterIoStatus;

        private readonly Task inverterReceiveTask;

        private readonly Task inverterSendTask;

        private readonly Dictionary<InverterIndex, IInverterStatusBase> inverterStatuses;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly ISocketTransport socketTransport;

        private readonly IVertimagConfiguration vertimagConfiguration;

        private Timer axisPositionUpdateTimer;

        private Axis currentAxis;

        private IInverterStateMachine currentStateMachine;

        private bool disposed;

        private bool forceStatusPublish;

        private Timer heartBeatTimer;

        private Timer sensorStatusUpdateTimer;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public HostedInverterDriver(IEventAggregator eventAggregator,
            ISocketTransport socketTransport,
            IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement,
            IVertimagConfiguration vertimagConfiguration,
            ILogger<HostedInverterDriver> logger)
        {
            logger.LogDebug( "1:Method Start" );

            this.socketTransport = socketTransport;
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.vertimagConfiguration = vertimagConfiguration;
            this.logger = logger;

            this.inverterStatuses = new Dictionary<InverterIndex, IInverterStatusBase>();

            this.inverterIoStatus = new InverterIoStatus();

            this.heartbeatQueue = new BlockingConcurrentQueue<InverterMessage>();
            this.inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

            this.commandQueue = new BlockingConcurrentQueue<FieldCommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

            this.commandReceiveTask = new Task( this.CommandReceiveTaskFunction );
            this.notificationReceiveTask = new Task( async () => await this.NotificationReceiveTaskFunction() );
            this.inverterReceiveTask = new Task( async () => await this.ReceiveInverterData() );
            this.inverterSendTask = new Task( async () => await this.SendInverterCommand() );

            this.logger.LogTrace( "2:Subscription Command" );

            this.InitializeMethodSubscriptions();
        }

        #endregion

        #region Destructors

        ~HostedInverterDriver()
        {
            this.Dispose( false );
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
                this.sensorStatusUpdateTimer?.Dispose();
                this.axisPositionUpdateTimer?.Dispose();
            }

            this.disposed = true;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.stoppingToken = stoppingToken;

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical( $"2:Exception: {ex.Message} while starting service threads" );

                //TEMP throw new InverterDriverException($"Exception: {ex.Message} while starting service threads", ex);

                this.SendMessage( new InverterExceptionMessageData( ex, "", 0 ) );
            }

            return Task.CompletedTask;
        }

        private void CommandReceiveTaskFunction()
        {
            this.logger.LogDebug( "1:Method Start" );

            this.sensorStatusUpdateTimer?.Dispose();
            this.sensorStatusUpdateTimer = new Timer( this.RequestSensorStatusUpdate, null, -1, Timeout.Infinite );

            this.axisPositionUpdateTimer?.Dispose();
            this.axisPositionUpdateTimer = new Timer( this.RequestAxisPositionUpdate, null, -1, Timeout.Infinite );

            do
            {
                FieldCommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue( Timeout.Infinite, this.stoppingToken, out receivedMessage );

                    this.logger.LogTrace( $"2:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}" );
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug( "3:Method End operation cancelled" );

                    return;
                }

                //TODO catch generic exception
                catch (Exception ex)
                {
                    this.logger.LogDebug( $"4:Exception: {ex.Message}" );

                    this.SendMessage( new InverterExceptionMessageData( ex, "", 0 ) );

                    return;
                }

                if (this.inverterStatuses.Count == 0)
                {
                    var errorNotification = new FieldNotificationMessage( null,
                        "Inverter Driver not configured jet",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        receivedMessage.Type,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical );

                    this.logger.LogTrace( $"4:Invert Driver not configured for message Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}" );

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish( errorNotification );

                    continue;
                }

                if (receivedMessage.Type == FieldMessageType.InverterStop)
                {
                    this.currentStateMachine?.Dispose();

                    ProcessStopMessage( receivedMessage );

                    continue;
                }

                if (this.currentStateMachine != null)
                {
                    var errorNotification = new FieldNotificationMessage( null,
                        "Inverter operation already in progress",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        receivedMessage.Type,
                        MessageStatus.OperationError,
                        ErrorLevel.Error );

                    this.logger.LogTrace( $"5:Inverter Driver already executing operation {this.currentStateMachine.GetType()} but received message Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}" );

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish( errorNotification );
                    continue;
                }

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.CalibrateAxis:
                        ProcessCalibrateAxisMessage( receivedMessage );
                        break;

                    case FieldMessageType.InverterPowerOff:
                        this.ProcessPowerOffMessage( receivedMessage );
                        break;

                    case FieldMessageType.InverterPowerOn:
                        ProcessPowerOnMessage( receivedMessage );
                        break;

                    //case FieldMessageType.Positioning:
                    //    if (receivedMessage.Data is IPositioningFieldMessageData positioningData)
                    //    {
                    //        this.logger.LogDebug($"7:Object creation");

                    //        this.currentAxis = positioningData.AxisMovement;
                    //        this.currentStateMachine = new PositioningStateMachine(positioningData, this.inverterCommandQueue, this.eventAggregator, this.logger);
                    //        this.currentStateMachine?.Start();
                    //    }
                    //    this.axisPositionUpdateTimer.Change(AXIS_POSITION_UPDATE_INTERVAL, AXIS_POSITION_UPDATE_INTERVAL);
                    //    break;

                    case FieldMessageType.ShutterPositioning:
                        ProcessShutterPositioningMessage(receivedMessage);
                        break;

                    case FieldMessageType.InverterStatusUpdate:
                        ProcessInverterStatusUpdateMessage( receivedMessage );
                        break;

                    case FieldMessageType.InverterSwitchOff:
                        ProcessInverterSwitchOffMessage( receivedMessage );
                        break;

                    case FieldMessageType.InverterSwitchOn:
                        ProcessInverterSwitchOnMessage( receivedMessage );
                        break;

                    default:
                        break;


                }
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task NotificationReceiveTaskFunction()
        {
            this.logger.LogDebug( "1:Method Start" );

            do
            {
                FieldNotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue( Timeout.Infinite, this.stoppingToken, out receivedMessage );

                    this.logger.LogTrace( $"2:Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}" );
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug( "3:Method End operation cancelled" );

                    return;
                }

                //TODO catch generic exception
                catch (Exception ex)
                {
                    this.logger.LogDebug( $"4:Exception: {ex.Message}" );

                    this.SendMessage( new InverterExceptionMessageData( ex, "", 0 ) );

                    return;
                }

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.DataLayerReady:

                        await this.StartHardwareCommunications();
                        await this.InitializeInverterStatus();

                        break;

                    case FieldMessageType.CalibrateAxis:
                    case FieldMessageType.ShutterPositioning:
                    case FieldMessageType.InverterPowerOff:
                    case FieldMessageType.InverterSwitchOn:
                    case FieldMessageType.InverterStop:

                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            this.currentStateMachine?.Dispose();
                            this.currentStateMachine = null;
                        }

                        break;

                    case FieldMessageType.InverterSwitchOff:
                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            this.currentStateMachine?.Dispose();
                            this.currentStateMachine = null;

                            var nextMessage = ((InverterSwitchOffFieldMessageData)receivedMessage.Data).NextCommandMessage;
                            if (nextMessage != null)
                            {
                                this.commandQueue.Enqueue( nextMessage );
                            }
                        }

                        break;

                    case FieldMessageType.InverterPowerOn:

                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            this.currentStateMachine?.Dispose();
                            this.currentStateMachine = null;

                            var nextMessage = ((InverterPowerOnFieldMessageData)receivedMessage.Data).NextCommandMessage;
                            if (nextMessage != null)
                            {
                                this.commandQueue.Enqueue( nextMessage );
                            }
                        }

                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task ReceiveInverterData()
        {
            this.logger.LogDebug( "1:Method Start" );

            do
            {
                byte[] inverterData;
                try
                {
                    inverterData = await this.socketTransport.ReadAsync( this.stoppingToken );
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug( "2:Method End operation cancelled" );

                    return;
                }

                //TODO catch generic exception
                catch (Exception ex)
                {
                    this.logger.LogDebug( $"3:Exception: {ex.Message}" );

                    this.SendMessage( new InverterExceptionMessageData( ex, "", 0 ) );

                    return;
                }

                //INFO: Byte 1 of read data contains packet length, zero means invalid packet
                if (inverterData == null)
                {
                    this.logger.LogTrace( $"3:Inverter message is null" );
                    continue;
                }
                if (inverterData[1] == 0x00)
                {
                    this.logger.LogTrace( $"4:Inverter message length is zero" );
                    continue;
                }

                InverterMessage currentMessage;
                try
                {
                    currentMessage = new InverterMessage( inverterData );

                    this.logger.LogTrace( $"5:currentMessage={currentMessage}" );
                }
                catch (InverterDriverException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    var errorNotification = new FieldNotificationMessage( null,
                        $"Exception {ex.Message} while parsing Inverter raw message bytes",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterException,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical );

                    this.logger.LogTrace( $"6:Exception {ex.Message} while parsing Inverter raw message bytes" );

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish( errorNotification );

                    this.SendMessage( new InverterExceptionMessageData( ex, "", 0 ) );

                    return;
                }

                if (!Enum.TryParse( currentMessage.SystemIndex.ToString(), out InverterIndex inverterIndex ))
                {
                    var errorNotification = new FieldNotificationMessage( null,
                        $"Invalid system index {currentMessage.SystemIndex} defined in Inverter Message",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterException,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical );

                    this.logger.LogTrace( $"7:Invalid system index {currentMessage.SystemIndex} defined in Inverter Message" );

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish( errorNotification );

                    return;
                }

                if (currentMessage.IsWriteMessage)
                {
                    this.logger.LogTrace( "8:Evaluate Write Message" );

                    this.EvaluateWriteMessage( currentMessage, inverterIndex );
                }

                if (currentMessage.IsReadMessage)
                {
                    this.logger.LogTrace( "9:Evaluate Read Message" );

                    this.EvaluateReadMessage( currentMessage, inverterIndex );
                }
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task SendInverterCommand()
        {
            this.logger.LogDebug( "1:Method Start" );

            //INFO Create WaitHandle array to wait for multiple events
            var commandHandles = new[]
            {
                this.heartbeatQueue.WaitHandle,
                this.inverterCommandQueue.WaitHandle
            };

            do
            {
                var handleIndex = WaitHandle.WaitAny( commandHandles );

                this.logger.LogTrace( $"2:handleIndex={handleIndex}" );

                switch (handleIndex)
                {
                    case 0:
                        await this.ProcessHeartbeat();
                        break;

                    case 1:
                        await this.ProcessInverterCommand();
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        private void SendMessage(IMessageData data)
        {
            var msg = new NotificationMessage(
                data,
                "Inverter Driver Error",
                MessageActor.Any,
                MessageActor.InverterDriver,
                MessageType.InverterException,
                MessageStatus.OperationError,
                ErrorLevel.Critical );
            this.eventAggregator.GetEvent<NotificationEvent>().Publish( msg );
        }

        #endregion
    }
}
