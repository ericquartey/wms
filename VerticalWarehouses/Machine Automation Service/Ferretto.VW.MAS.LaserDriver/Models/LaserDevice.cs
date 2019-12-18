using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.LaserDriver
{
    internal sealed class LaserDevice : ILaserDevice
    {
        #region Fields

        private readonly CancellationToken cancellationToken;

        //private readonly Task laserReceiveTask;

        //private readonly Task laserSendTask;

        private readonly ILogger logger;

        private readonly ISocketTransport socketTransport;

        #endregion

        #region Constructors

        public LaserDevice(BayNumber bayNumber, IPAddress ipAddress, int port,
            ISocketTransport transport, ILogger logger, CancellationToken cancellationToken)
        {
            this.BayNumber = bayNumber;
            this.IpAddress = ipAddress;
            this.TcpPort = port;

            this.socketTransport = transport;
            this.logger = logger;
            this.cancellationToken = cancellationToken;

            //this.laserReceiveTask = new Task(async () => await this.ReceiveLaserDataTaskFunction());
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

                //this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", (int)ex.ExceptionCode));
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Fatal error while connecting to Laser {this.BayNumber}: {ex.Message}");

                //this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", 0), isFatalError: true);

                return;
            }

            if (!this.socketTransport.IsConnected)
            {
                this.logger.LogError($"3:Failed to connect to Laser {this.BayNumber}");

                //this.SendOperationErrorMessage(new IoExceptionFieldMessageData(null, "Socket Transport failed to connect", (int)IoDriverExceptionCode.DeviceNotConnected));
            }
            else
            {
                this.logger.LogInformation($"Connection OK to Laser {this.BayNumber} on TCP address {this.IpAddress}:{this.TcpPort}");
            }

            try
            {
                //this.laserReceiveTask.Start();

                //this.laserSendTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"4:Exception: {ex.Message} while starting service hardware threads");

                //this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "Laser Driver Exception", 0), isFatalError: true);

                return;
            }

            //this.StartPollingIoMessage();
        }

        private async Task ReceiveLaserDataTaskFunction()
        {
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
