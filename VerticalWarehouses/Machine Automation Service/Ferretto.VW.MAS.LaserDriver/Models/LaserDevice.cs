using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.LaserDriver
{
    internal sealed class LaserDevice : ILaserDevice
    {
        #region Fields

        private readonly CancellationToken cancellationToken;

        private readonly BlockingConcurrentQueue<FieldCommandMessage> laserCommandQueue = new BlockingConcurrentQueue<FieldCommandMessage>();

        private readonly Task laserReceiveTask;

        private readonly Task laserSendTask;

        private readonly ILogger logger;

        private readonly ISocketTransport socketTransport;

        private readonly ManualResetEventSlim writeEnableEvent;

        private byte[] receiveBuffer;

        #endregion

        #region Constructors

        public LaserDevice(BayNumber bayNumber, IPAddress ipAddress, int port,
            ISocketTransport transport, ILogger logger, CancellationToken cancellationToken)
        {
            this.BayNumber = bayNumber;
            this.IpAddress = ipAddress;
            this.TcpPort = port;

            this.writeEnableEvent = new ManualResetEventSlim(true);
            this.socketTransport = transport;
            this.logger = logger;
            this.cancellationToken = cancellationToken;

            this.laserReceiveTask = new Task(async () => await this.ReceiveLaserDataTaskFunction());
            //this.laserSendTask = new Task(async () => await this.SendLaserCommandTaskFunction());
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; }

        public IPAddress IpAddress { get; }

        public int TcpPort { get; }

        #endregion

        #region Methods

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
            }
            else
            {
                this.logger.LogInformation($"Connection OK to Laser {this.BayNumber} on TCP address {this.IpAddress}:{this.TcpPort}");
            }

            try
            {
                this.laserReceiveTask.Start();

                //this.laserSendTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"4:Exception: {ex.Message} while starting service hardware threads");
                return;
            }

            //this.StartPollingIoMessage();
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
                    catch (Exception ex)
                    {
                        this.logger.LogCritical($"Error while connecting to Laser {this.BayNumber}");

                        return;
                    }

                    if (!this.socketTransport.IsConnected)
                    {
                        this.logger.LogError("3:Socket Transport failed to connect");
                        continue;
                    }
                    else
                    {
                        this.logger.LogInformation($"3:Connection OK Laser {this.BayNumber} on {this.IpAddress}:{this.TcpPort}");
                    }

                    this.writeEnableEvent.Set();

                    this.laserCommandQueue.Enqueue(new FieldCommandMessage(null, string.Empty, FieldMessageActor.LaserDriver, FieldMessageActor.LaserDriver, FieldMessageType.LaserOff, (byte)this.BayNumber));
                }

                byte[] telegram;
                try
                {
                    telegram = await this.socketTransport.ReadAsync(this.cancellationToken);

                    if (telegram == null || telegram.Length == 0)
                    {
                        // connection error
                        this.logger.LogError($"4:Laser Driver message is null");
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
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
            while (!this.cancellationToken.IsCancellationRequested);
        }

        #endregion
    }
}
