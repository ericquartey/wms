using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;

namespace Ferretto.VW.Devices.LaserPointer
{
    internal sealed class LaserPointerDriver : ILaserPointerDriver
    {
        #region Fields

        public const string IP_ADDRESS_DEFAULT = "192.168.99.12";

        public const int PORT_DEFAULT = 2020;

        private const string NEW_LINE = "\r\n";

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly int homeTimeout = 20000;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentQueue<string> messagesReceivedQueue;

        private readonly ConcurrentQueue<string> messagesToBeSendQueue;

        private readonly int tcpTimeout = 2000;

        private TcpClient client;

        private IPAddress ipAddress = IPAddress.Parse(IP_ADDRESS_DEFAULT);

        private bool laserEnabled = false;

        private int port = PORT_DEFAULT;

        //private LaserPointerCommands.Command setPositionStatus = LaserPointerCommands.Command.SETP_F;

        private NetworkStream stream;

        private bool testEnabled = false;

        private double xOffset;

        private double yOffset;

        private double zOffsetLowerPosition;

        private double zOffsetUpperPosition;

        #endregion

        #region Constructors

        public LaserPointerDriver()
        {
            this.messagesToBeSendQueue = new ConcurrentQueue<string>();
            this.messagesReceivedQueue = new ConcurrentQueue<string>();
        }

        #endregion

        #region Properties

        public IPAddress IpAddress => this.ipAddress;

        public bool IsConnected => this.client?.Connected ?? false;

        public int Port => this.port;

        public LaserPoint SelectedPoint { get; set; }

        public bool TestEnabled => this.testEnabled;

        #endregion

        #region Methods

        public LaserPoint CalculateLaserPoint(double loadingUnitWidth, double loadingUnitDepth, double compartmentWidth, double compartmentDepth, double compartmentXPosition, double compartmentYPosition, double missionOperationItemHeight, bool isBayUpperPosition, WarehouseSide baySide)
        {
            var result = new LaserPoint();

            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};LaserPointerDriver;CalculateLaserPoint();loadingUnitWidth: {loadingUnitWidth}, loadingUnitDepth {loadingUnitDepth}");
            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};LaserPointerDriver;CalculateLaserPoint();compartmentWidth: {compartmentWidth}, compartmentDepth {compartmentDepth}");
            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};LaserPointerDriver;CalculateLaserPoint();compartmentXPosition: {compartmentXPosition}, compartmentYPosition {compartmentYPosition}, missionOperationItemHeight {missionOperationItemHeight}");
            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};LaserPointerDriver;CalculateLaserPoint();xOffser: {this.xOffset}, yOffset {this.yOffset}, zOffsetLow {this.zOffsetLowerPosition}, zOffsetUp {this.zOffsetUpperPosition} baySide {baySide} ");

            result.X = (int)Math.Round((loadingUnitWidth / 2) - compartmentXPosition - (compartmentWidth / 2) + this.xOffset);
            result.Y = -1 * (int)Math.Round((loadingUnitDepth / 2) - compartmentYPosition - (compartmentDepth / 2) + this.yOffset);

            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};LaserPointerDriver;CalculateLaserPoint();X: {result.X}");
            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};LaserPointerDriver;CalculateLaserPoint();Y: {result.Y}");

            if (baySide == WarehouseSide.Back)
            {
                result.X = -1 * result.X;
                result.Y = -1 * result.Y;
            }

            result.Z = isBayUpperPosition ? (int)this.zOffsetUpperPosition : (int)this.zOffsetLowerPosition;
            result.Z -= (int)Math.Round(missionOperationItemHeight);

            result.Speed = LaserPoint.SPEEDDEFAULT;

            return result;
        }

        public LaserPoint CalculateLaserPointForSocketLink(int x, int y, int z, MachineIdentity machineIdentity, bool isBayUpperPosition, WarehouseSide baySide)
        {
            var result = new LaserPoint();

            result.X = (int)Math.Round((machineIdentity.LoadingUnitWidth / 2) - x + this.xOffset);
            result.Y = -1 * (int)Math.Round((machineIdentity.LoadingUnitDepth / 2) - y + this.yOffset);
            result.Z = this.GetZ(z, isBayUpperPosition);
            result = this.GetXYBack(result, baySide);
            result.Speed = LaserPoint.SPEEDDEFAULT;

            return result;
        }

        public void ClearCommands()
        {
            this.ClearConcurrentQueue(this.messagesToBeSendQueue);
            this.SelectedPoint = null;
        }

        public bool Configure(IPAddress ipAddress, int port, double xOffset = 0, double yOffset = 0, double zOffsetLowerPosition = 0, double zOffsetUpperPosition = 0)
        {
            if (ipAddress != this.ipAddress || port != this.port)
            {
                this.DisconnectAsync();
            }
            this.ipAddress = ipAddress;
            this.port = port;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.zOffsetUpperPosition = zOffsetUpperPosition;
            this.zOffsetLowerPosition = zOffsetLowerPosition;

            return true;
        }

        public async Task ConnectAsync()
        {
            await this._semaphore.WaitAsync();
            try
            {
                if (this.client is null)
                {
                    this.client = new TcpClient();
                    this.stream = null;
                }
                if (!this.IsConnected)
                {
                    this.logger.Debug($"Connect");
                    this.client.SendTimeout = this.tcpTimeout;
                    await this.client.ConnectAsync(this.IpAddress, this.Port);
                    if (this.IsConnected)
                    {
                        this.stream = this.client.GetStream();
                        this.logger.Debug($"Connected");
                    }
                    //this.SelectedPoint = null;
                }
                this._semaphore.Release();
            }
            catch (Exception e)
            {
                this._semaphore.Release();
                this.logger.Error(e);
                await this.DisconnectAsync();
            }
        }

        public async Task DisconnectAsync()
        {
            await this._semaphore.WaitAsync();
            try
            {
                this.logger.Debug($"Disconnect");
                this.stream?.Close();
                this.client?.Close();
                this.client = null;
                this._semaphore.Release();
            }
            catch (Exception e)
            {
                this._semaphore.Release();
                this.logger.Error(e);
            }
        }

        /// <summary>
        /// Switch on or off the laser pointer.
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public async Task<bool> EnabledAsync(bool enable, bool onMovement)
        {
            //if (enable == this.laserEnabled)
            //{
            //    return true;
            //}

            if (enable)
            {
                if (onMovement)
                {
                    this.EnqueueCommand(LaserPointerCommands.Command.LASER_MOVE_ON);
                }
                else
                {
                    this.EnqueueCommand(LaserPointerCommands.Command.LASER_ON);
                }
            }
            else
            {
                if (onMovement)
                {
                    this.EnqueueCommand(LaserPointerCommands.Command.LASER_MOVE_OFF);
                }
                else
                {
                    this.EnqueueCommand(LaserPointerCommands.Command.LASER_OFF);
                }

                this.SelectedPoint = null;
            }

            return true;
        }

        /// <summary>
        /// Send the messages int the queue, in the right order.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ExecuteCommandsAsync(CancellationToken? cancellationToken)
        {
            var result = false;
            try
            {
                this.ClearConcurrentQueue(this.messagesReceivedQueue);
                int errors = 0;

                while (!this.messagesToBeSendQueue.IsEmpty
                    && (!cancellationToken.HasValue || !cancellationToken.Value.IsCancellationRequested)
                    )
                {
                    if (this.messagesToBeSendQueue.TryPeek(out var sendMessage))
                    {
                        var ackReceived = false;
                        if (!this.IsConnected)
                        {
                            await this.ConnectAsync();
                        }

                        if (this.IsConnected)
                        {
                            this.logger.Trace($"ExecuteCommands();Write");
                            var data = Encoding.ASCII.GetBytes(sendMessage);
                            this.stream = this.client.GetStream();
                            var timeout = sendMessage.Contains("HOME") ? this.homeTimeout : this.tcpTimeout;
                            this.stream.ReadTimeout = timeout;
                            this.stream.WriteTimeout = timeout;
                            this.stream.Write(data, 0, data.Length);
                            this.logger.Debug($"ExecuteCommands();Sent: {sendMessage.Replace("\r", "<CR>").Replace("\n", "<LF>")}");

                            if (this.IsConnected && this.IsWaitResponse(sendMessage))
                            {
                                data = new byte[this.client.ReceiveBufferSize];
                                var bytes = 0;
                                var responseMessage = "";
                                try
                                {
                                    bytes = this.stream.Read(data, 0, data.Length);
                                    responseMessage = Encoding.ASCII.GetString(data, 0, bytes);

                                    this.messagesReceivedQueue.Enqueue(responseMessage);
                                    this.logger.Debug($"ExecuteCommands();Received: {responseMessage.Replace("\r", "<CR>").Replace("\n", "<LF>")}");
                                }
                                catch (Exception e)
                                {
                                    this.logger.Debug(e);
                                }
                                if (bytes <= 0 || !this.IsResponseOk(sendMessage, responseMessage))
                                {
                                    this.logger.Debug($"ExecuteCommands;ArgumentException;{sendMessage.Replace("\r", "<CR>").Replace("\n", "<LF>")},{responseMessage.Replace("\r", "<CR>").Replace("\n", "<LF>")}");
                                    if (errors++ > 5)
                                    {
                                        this.ClearCommands();
                                        this.logger.Error($"ExecuteCommands: too many errors!");
                                        await this.DisconnectAsync();
                                        Thread.Sleep(400);
                                        break;
                                    }
                                }
                                else
                                {
                                    ackReceived = true;
                                }
                            }
                            else
                            {
                                //this.logger.Debug($"ArgumentException;no wait {sendMessage}");
                                this.messagesReceivedQueue.Enqueue("");
                                ackReceived = true;
                            }
                        }
                        else
                        {
                            // retry
                            Thread.Sleep(1000);
                            await this.ConnectAsync();
                            if (!this.IsConnected)
                            {
                                this.logger.Error("Device not ready");
                                this.ClearCommands();
                                break;
                            }
                        }
                        if (ackReceived)
                        {
                            this.messagesToBeSendQueue.TryDequeue(out _);
                            errors = 0;
                            result = true;
                        }
                    }
                    else
                    {
                        this.logger.Debug("queue locked");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                this.ClearCommands();
                this.logger.Error(e);
                await this.DisconnectAsync();
                Thread.Sleep(400);
            }
            return result;
        }

        public async Task<bool> HelpAsync()
        {
            this.EnqueueCommand(LaserPointerCommands.Command.HELP);
            return true;
        }

        public async Task<bool> HomeAsync()
        {
            this.EnqueueCommand(LaserPointerCommands.Command.HOME);
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="JogCommand"></param>
        /// <returns></returns>
        public async Task<bool> JogAsync(LaserPointerCommands.Command JogCommand)
        {
            switch (JogCommand)
            {
                case LaserPointerCommands.Command.JOGX_ON0:
                case LaserPointerCommands.Command.JOGX_ON1:
                case LaserPointerCommands.Command.JOGX_OFF:
                case LaserPointerCommands.Command.JOGY_ON0:
                case LaserPointerCommands.Command.JOGY_ON1:
                case LaserPointerCommands.Command.JOGY_OFF:
                    this.EnqueueCommand(JogCommand);
                    break;

                default:
                    return false;
            }

            return true;
        }

        public async Task<bool> MoveAndSwitchOnAsync(LaserPoint point, bool select = false)
        {
            if (point is null)
            {
                return false;
            }

            if (this.SelectedPoint is null
                || !select
                || point.X != this.SelectedPoint.X
                || point.Y != this.SelectedPoint.Y
                || point.Z != this.SelectedPoint.Z)
            {
                if (select)
                {
                    this.SelectedPoint = point;
                }
                //else
                //{
                //    this.ClearCommands();
                //}

                this.EnqueueCommand(LaserPointerCommands.Command.MOVE, point);
                this.EnqueueCommand(LaserPointerCommands.Command.LASER_ON, point);

                return true;
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public async Task<bool> MoveAsync(LaserPoint point)
        {
            if (point is null)
            {
                return false;
            }

            this.EnqueueCommand(LaserPointerCommands.Command.MOVE, point);
            return true;
        }

        public async Task<bool> ParametersAsync()
        {
            var result = false;

            try
            {
                var request = WebRequest.Create(new Uri($"http://{this.IpAddress}/parameters.txt"));
                request.Timeout = 1000;
                request.Credentials = CredentialCache.DefaultCredentials;
                var response = request.GetResponse();

                using (var dataStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(dataStream);
                    var responseFromServer = reader.ReadToEnd();
                }

                response.Close();

                result = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};LaserPointerDriver;ParametersAsync;{e.Message}");
            }

            return result;
        }

        public async Task<bool> PositionAsync(LaserSetPosition position)
        {
            this.EnqueueCommand(LaserPointerCommands.Command.STEP, null, null, position);
            return true;
        }

        public async Task<bool> PositionFinishAsync()
        {
            this.EnqueueCommand(LaserPointerCommands.Command.SETP_F);
            return true;
        }

        public async Task<bool> PositionInitializeAsync()
        {
            this.EnqueueCommand(LaserPointerCommands.Command.SETP_I);
            return true;
        }

        public async Task<bool> PositionSaveAsync()
        {
            this.EnqueueCommand(LaserPointerCommands.Command.SETP_S);
            return true;
        }

        public void ResetSelectedPoint()
        {
            this.logger.Debug($"Reset point");
            this.SelectedPoint = null;
        }

        public async Task<bool> StepAsync(LaserStep step)
        {
            this.EnqueueCommand(LaserPointerCommands.Command.STEP, null, step);
            return true;
        }

        public async Task<bool> SwitchOnAndMoveAsync(LaserPoint point)
        {
            if (point is null)
            {
                return false;
            }

            this.EnqueueCommand(LaserPointerCommands.Command.LASER_ON, point);
            this.EnqueueCommand(LaserPointerCommands.Command.MOVE, point);
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> TestAsync(bool enable)
        {
            if (enable)
            {
                this.EnqueueCommand(LaserPointerCommands.Command.LASER_ON);
                this.EnqueueCommand(LaserPointerCommands.Command.TEST_ON);
            }
            else
            {
                this.EnqueueCommand(LaserPointerCommands.Command.TEST_OFF);
                this.EnqueueCommand(LaserPointerCommands.Command.LASER_OFF);
            }

            return true;
        }

        private bool ClearConcurrentQueue(ConcurrentQueue<string> concurrentQueure)
        {
            while (concurrentQueure.TryDequeue(out var sendMessage)) {; }
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="command"></param>
        /// <param name="point"></param>
        /// <param name="step"></param>
        /// <param name="setPosition"></param>
        /// <returns></returns>
        private bool EnqueueCommand(LaserPointerCommands.Command command, LaserPoint point = null, LaserStep step = null, LaserSetPosition setPosition = null)
        {
            var result = false;
            var strCommand = command.ToString();

            switch (command)
            {
                case LaserPointerCommands.Command.HOME:
                    strCommand = "HOME";
                    break;

                case LaserPointerCommands.Command.LASER_ON:
                    strCommand = "LASER ON";
                    this.laserEnabled = true;
                    break;

                case LaserPointerCommands.Command.LASER_OFF:
                    strCommand = "LASER OFF";
                    this.laserEnabled = false;
                    break;

                case LaserPointerCommands.Command.TEST_ON:                   // deprecated, not use
                    strCommand = "TEST ON";
                    this.testEnabled = true;
                    break;

                case LaserPointerCommands.Command.TEST_OFF:
                    strCommand = "TEST OFF";
                    this.testEnabled = false;
                    break;

                case LaserPointerCommands.Command.JOGX_ON0:
                    strCommand = "JOGX ON0";
                    break;

                case LaserPointerCommands.Command.JOGX_ON1:
                    strCommand = "JOGX ON1";
                    break;

                case LaserPointerCommands.Command.JOGX_OFF:
                    strCommand = "JOGX OFF";
                    break;

                case LaserPointerCommands.Command.JOGY_ON0:
                    strCommand = "JOGY ON0";
                    break;

                case LaserPointerCommands.Command.JOGY_ON1:
                    strCommand = "JOGY ON1";
                    break;

                case LaserPointerCommands.Command.JOGY_OFF:
                    strCommand = "JOGY OFF";
                    break;

                case LaserPointerCommands.Command.MOVE:
                    strCommand += " " + point.ToString();
                    break;

                case LaserPointerCommands.Command.STEP:
                    strCommand += " " + step.ToString();
                    break;

                case LaserPointerCommands.Command.SETP:
                    strCommand += " " + setPosition.ToString();
                    break;

                //case LaserPointerCommands.Command.SETP_I:
                //    this.setPositionStatus = LaserPointerCommands.Command.SETP_I;
                //    strCommand = "SETP I";
                //    break;

                //case LaserPointerCommands.Command.SETP_F:
                //    this.setPositionStatus = LaserPointerCommands.Command.SETP_F;
                //    strCommand = "SETP F";
                //    break;

                case LaserPointerCommands.Command.SETP_S:
                    strCommand = "SETP S";
                    break;

                case LaserPointerCommands.Command.SETP_P:
                    strCommand = "SETP P";
                    break;
            }

            strCommand += NEW_LINE;

            this.messagesToBeSendQueue.Enqueue(strCommand);
            result = true;

            return result;
        }

        private LaserPoint GetXYBack(LaserPoint point, WarehouseSide baySide)
        {
            var result = point;

            if (baySide == WarehouseSide.Back)
            {
                result.X = -1 * result.X;
                result.Y = -1 * result.Y;
            }
            return result;
        }

        private int GetZ(int z, bool isBayUpperPosition)
        {
            var result = isBayUpperPosition ? (int)this.zOffsetUpperPosition : (int)this.zOffsetLowerPosition;
            return result - z;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="msgSend"></param>
        /// <param name="msgReceive"></param>
        /// <returns></returns>
        private bool IsResponseOk(string msgSend, string msgReceive)
        {
            var result = false;

            if (string.IsNullOrEmpty(msgSend) || string.IsNullOrEmpty(msgReceive))
            {
                return true;
            }

            msgReceive = msgReceive.Trim();

            if (msgSend.StartsWith("GET", StringComparison.Ordinal))
            {
                if (msgReceive.StartsWith("GET", StringComparison.Ordinal))
                {
                    result = true;
                }
            }
            else if (msgSend.StartsWith("TEST", StringComparison.Ordinal))
            {
                if (msgReceive.StartsWith("TEST", StringComparison.Ordinal) ||
                    msgReceive.StartsWith("OK", StringComparison.Ordinal)
                    )
                {
                    result = true;
                }
            }
            else if (msgSend.StartsWith("HELP", StringComparison.Ordinal))
            {
                if (msgReceive.StartsWith("DIM", StringComparison.Ordinal))
                {
                    result = true;
                }
            }
            else
            {
                if (msgReceive.StartsWith("OK", StringComparison.Ordinal))
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Check if the client must wait the response from the server.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool IsWaitResponse(string message)
        {
            var result = true;

            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            if (message.StartsWith("CLEAR", StringComparison.Ordinal)
                || message.StartsWith("ENABLE", StringComparison.Ordinal)
                //|| message.StartsWith("LASER OFF", StringComparison.Ordinal)
                //|| message.StartsWith("MOVE", StringComparison.Ordinal)
                )
            {
                result = false;
            }

            return result;
        }

        #endregion
    }
}
