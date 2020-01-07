using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.LaserDriver.StateMachines.MoveAndSwitchOn;
using Ferretto.VW.MAS.LaserDriver.StateMachines.SwitchOff;
using Ferretto.VW.MAS.LaserDriver.StateMachines.SwitchOn;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.LaserDriver
{
    internal sealed class LaserDevice : ILaserDevice, IDisposable
    {
        #region Fields

        private readonly CancellationToken cancellationToken;

        private readonly IErrorsProvider errorsProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<FieldCommandMessage> laserCommandQueue = new BlockingConcurrentQueue<FieldCommandMessage>();

        private readonly Task laserReceiveTask;

        private readonly Task laserSendTask;

        private readonly ILogger logger;

        private readonly ISocketTransport socketTransport;

        private readonly ManualResetEventSlim writeEnableEvent;

        private ILaserStateMachine currentStateMachine;

        private bool disposedValue = false;

        private byte[] receiveBuffer;

        #endregion

        #region Constructors

        public LaserDevice(
            BayNumber bayNumber,
            IPAddress ipAddress,
            int port,
            ISocketTransport transport,
            IEventAggregator eventAggregator,
            IErrorsProvider errorsProvider,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            this.BayNumber = bayNumber;
            this.IpAddress = ipAddress;
            this.TcpPort = port;

            this.writeEnableEvent = new ManualResetEventSlim(true);
            this.socketTransport = transport;
            this.logger = logger;
            this.cancellationToken = cancellationToken;
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.eventAggregator = eventAggregator;

            this.laserReceiveTask = new Task(async () => await this.ReceiveLaserDataTaskFunction());
            this.laserSendTask = new Task(async () => await this.SendLaserCommandTaskFunction());
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; }

        public IPAddress IpAddress { get; }

        public int TcpPort { get; }

        private ILaserStateMachine CurrentStateMachine
        {
            get => this.currentStateMachine;
            set
            {
                if (this.currentStateMachine != value)
                {
                    this.currentStateMachine = value;
                }
            }
        }

        #endregion

        #region Methods

        public void DestroyStateMachine()
        {
            if (this.CurrentStateMachine is IDisposable stateMachine)
            {
                stateMachine.Dispose();
            }

            this.CurrentStateMachine = null;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public void ExecuteLaserMoveAndSwitchOn()
        {
            if (this.CurrentStateMachine != null)
            {
                this.logger.LogInformation($"Laser {this.BayNumber} already executing operation {this.CurrentStateMachine.GetType().Name}");

                var ex = new Exception();
                //this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "Io Driver already executing operation", 0));
            }
            else
            {
                this.CurrentStateMachine = new MoveAndSwitchOnStateMachine(
                    this.BayNumber,
                    this.eventAggregator,
                    this.logger,
                    this.laserCommandQueue);

                this.CurrentStateMachine.Start();
            }
        }

        public void ExecuteLaserOff()
        {
            if (this.CurrentStateMachine != null)
            {
                this.logger.LogInformation($"Laser {this.BayNumber} already executing operation {this.CurrentStateMachine.GetType().Name}");

                var ex = new Exception();
                //this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "Io Driver already executing operation", 0));
            }
            else
            {
                this.CurrentStateMachine = new SwitchOffStateMachine(
                    this.BayNumber,
                    this.eventAggregator,
                    this.logger,
                    this.laserCommandQueue);

                this.CurrentStateMachine.Start();
            }
        }

        public void ExecuteLaserOn()
        {
            if (this.CurrentStateMachine != null)
            {
                this.logger.LogInformation($"Laser {this.BayNumber} already executing operation {this.CurrentStateMachine.GetType().Name}");

                var ex = new Exception();
                //this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "Io Driver already executing operation", 0));
            }
            else
            {
                this.CurrentStateMachine = new SwitchOnStateMachine(
                    this.BayNumber,
                    this.eventAggregator,
                    this.logger,
                    this.laserCommandQueue);

                this.CurrentStateMachine.Start();
            }
        }

        public async Task StartHardwareCommunicationsAsync()
        {
            this.logger.LogInformation($"1:Configure Laser {this.BayNumber}, tcp-endpoint={this.IpAddress}:{this.TcpPort}");

            try
            {
                await this.socketTransport.ConnectAsync(this.IpAddress, this.TcpPort);
            }
            catch (SocketTransportException ex)
            {
                this.logger.LogError($"2:Exception: {ex.Message} while connecting to Laser {this.BayNumber} - ExceptionCode: {ex.ExceptionCode};\nInner exception: {ex.InnerException.Message}");
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Fatal error while connecting to Laser {this.BayNumber}: {ex.Message}");
                return;
            }

            if (!this.socketTransport.IsConnected)
            {
                this.logger.LogError($"3:Failed to connect to Laser {this.BayNumber}");
                this.errorsProvider.RecordNew(DataModels.MachineErrorCode.LaserConnectionError, this.BayNumber);
            }
            else
            {
                this.logger.LogInformation($"Connection OK to Laser {this.BayNumber} on TCP address {this.IpAddress}:{this.TcpPort}");
            }

            try
            {
                this.laserReceiveTask.Start();

                this.laserSendTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"4:Exception: {ex.Message} while starting service hardware threads");
                return;
            }

            this.ExecuteLaserOff();
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this.laserCommandQueue?.Dispose();
                    this.laserReceiveTask?.Dispose();
                    this.laserSendTask?.Dispose();
                    this.writeEnableEvent?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        private async Task ReceiveLaserDataTaskFunction()
        {
            this.logger.LogTrace("1:Method Start");
            do
            {
                if (!this.socketTransport.IsConnected)
                {
                    try
                    {
                        this.receiveBuffer = null;
                        await this.socketTransport.ConnectAsync(this.IpAddress, this.TcpPort);
                    }
                    catch (SocketTransportException ex)
                    {
                        this.logger.LogError($"2:Exception: {ex.Message} while connecting to Laser {this.BayNumber} - ExceptionCode: {ex.ExceptionCode};\nInner exception: {ex.InnerException.Message}");
                    }
                    catch
                    {
                        this.logger.LogCritical($"Error while connecting to Laser {this.BayNumber}");

                        return;
                    }

                    if (!this.socketTransport.IsConnected)
                    {
                        this.logger.LogError("3:Socket Transport failed to connect");
                        this.errorsProvider.RecordNew(DataModels.MachineErrorCode.LaserConnectionError, this.BayNumber);
                        continue;
                    }
                    else
                    {
                        this.logger.LogInformation($"3:Connection OK Laser {this.BayNumber} on {this.IpAddress}:{this.TcpPort}");
                    }

                    this.writeEnableEvent.Set();

                    this.ExecuteLaserOff();
                }

                byte[] telegram;
                try
                {
                    telegram = await this.socketTransport.ReadAsync(this.cancellationToken);

                    if (telegram == null || telegram.Length == 0)
                    {
                        // connection error
                        this.logger.LogError($"4:Laser Driver message is null");
                        this.errorsProvider.RecordNew(DataModels.MachineErrorCode.LaserConnectionError, this.BayNumber);
                        var ex = new Exception();
                        continue;
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
                {
                    this.logger.LogDebug($"Terminating Laser Device {this.BayNumber} read thread.");

                    return;
                }
                catch (SocketTransportException ex)
                {
                    // connection error
                    this.logger.LogError(ex, $"3:Exception: {ex.Message} while connecting to Laser {this.BayNumber} - ExceptionCode: {ex.ExceptionCode}; Inner exception: {ex.InnerException?.Message ?? string.Empty}");
                    this.errorsProvider.RecordNew(DataModels.MachineErrorCode.LaserConnectionError, this.BayNumber);
                    continue;
                }
                catch (Exception ex)
                {
                    this.logger.LogCritical($"Fatal error for Laser device {this.BayNumber} while reading message {ex.Message}");
                    return;
                }

                this.receiveBuffer = this.receiveBuffer.AppendArrays(telegram, telegram.Length);

                var extractedMessages = BufferUtility.GetMessagesToEnqueue(ref this.receiveBuffer, null, new byte[] { 13, 10 });
                if (this.receiveBuffer.Length > 0)
                {
                    this.logger.LogWarning($"Message extracted: count {extractedMessages.Count}: left bytes {this.receiveBuffer.Length}");
                }

                if (extractedMessages.Count > 0)
                {
                    this.writeEnableEvent.Set();
                }

                foreach (var extractedMessage in extractedMessages)
                {
                    this.CurrentStateMachine?.ProcessResponseMessage(extractedMessage);
                }
            }
            while (!this.cancellationToken.IsCancellationRequested);
        }

        private async Task SendLaserCommandTaskFunction()
        {
            do
            {
                if (this.socketTransport.IsConnected)
                {
                    try
                    {
                        LaserFieldMessageData data = null;
                        if (this.laserCommandQueue.TryPeek(Timeout.Infinite, this.cancellationToken, out var message) && message != null)
                        {
                            this.logger.LogTrace($"1:message={message}: index {this.BayNumber}");

                            data = message.Data as LaserFieldMessageData;
                        }

                        if (message != null && this.writeEnableEvent.Wait(Timeout.Infinite, this.cancellationToken))
                        {
                            if (this.socketTransport.IsConnected)
                            {
                                this.writeEnableEvent.Reset();

                                var isWriteSuccessful = false;

                                switch (message.Type)
                                {
                                    case FieldMessageType.LaserOff:
                                        {
                                            var telegram = Encoding.ASCII.GetBytes("LASER OFF\r\n");
                                            isWriteSuccessful = await this.socketTransport.WriteAsync(telegram, this.cancellationToken) == telegram.Length;
                                        }
                                        break;

                                    case FieldMessageType.LaserOn:
                                        {
                                            var telegram = Encoding.ASCII.GetBytes("LASER ON\r\n");
                                            isWriteSuccessful = await this.socketTransport.WriteAsync(telegram, this.cancellationToken) == telegram.Length;
                                        }
                                        break;

                                    case FieldMessageType.LaserMove:
                                        {
                                            var telegram = Encoding.ASCII.GetBytes($"MOVE X={data.X}, Y={data.Y}, Z={data.Z}, V={data.Speed}\r\n");
                                            isWriteSuccessful = await this.socketTransport.WriteAsync(telegram, this.cancellationToken) == telegram.Length;
                                        }
                                        break;
                                }

                                if (isWriteSuccessful)
                                {
                                    this.laserCommandQueue.Dequeue(out _);
                                }
                                else
                                {
                                    this.writeEnableEvent.Set();
                                }
                            }
                            else
                            {
                                Thread.Sleep(5);
                            }
                        }
                    }
                    catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
                    {
                        this.logger.LogDebug($"Terminating Laser {this.BayNumber} write thread.");

                        return;
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
            while (!this.cancellationToken.IsCancellationRequested);
        }

        #endregion

        // To detect redundant calls
    }
}
