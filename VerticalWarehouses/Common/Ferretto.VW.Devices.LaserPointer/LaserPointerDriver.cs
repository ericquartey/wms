using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.Devices.LaserPointer
{
    internal sealed class LaserPointerDriver : ILaserPointerDriver
    {
        #region Fields

        public const string IP_ADDRESS_DEFAULT = "192.168.99.12";

        public const int PORT_DEFAULT = 2020;

        private const string NEW_LINE = "\r\n";

        private readonly ConcurrentQueue<string> errorsQueue;

        private readonly ConcurrentQueue<string> messagesReceivedQueue;

        private readonly ConcurrentQueue<string> messagesToBeSendQueue;

        private readonly int tcpTimeout = 2000;

        private IPAddress ipAddress = IPAddress.Parse(IP_ADDRESS_DEFAULT);

        private bool laserEnabled = false;

        private int port = PORT_DEFAULT;

        //private LaserPointerCommands.Command setPositionStatus = LaserPointerCommands.Command.SETP_F;

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
            this.errorsQueue = new ConcurrentQueue<string>();
        }

        #endregion

        #region Properties

        public IPAddress IpAddress => this.ipAddress;

        public int Port => this.port;

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

        public bool Configure(IPAddress ipAddress, int port, double xOffset = 0, double yOffset = 0, double zOffsetLowerPosition = 0, double zOffsetUpperPosition = 0)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.zOffsetUpperPosition = zOffsetUpperPosition;
            this.zOffsetLowerPosition = zOffsetLowerPosition;

            return true;
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
            }

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> HelpAsync()
        {
            this.EnqueueCommand(LaserPointerCommands.Command.HELP);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> HomeAsync()
        {
            this.EnqueueCommand(LaserPointerCommands.Command.HOME);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
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

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> MoveAndSwitchOnAsync(LaserPoint point)
        {
            if (point is null)
            {
                return false;
            }

            this.EnqueueCommand(LaserPointerCommands.Command.MOVE, point);
            this.EnqueueCommand(LaserPointerCommands.Command.LASER_ON, point);

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
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
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
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
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> PositionFinishAsync()
        {
            this.EnqueueCommand(LaserPointerCommands.Command.SETP_F);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> PositionInitializeAsync()
        {
            this.EnqueueCommand(LaserPointerCommands.Command.SETP_I);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> PositionSaveAsync()
        {
            this.EnqueueCommand(LaserPointerCommands.Command.SETP_S);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> StepAsync(LaserStep step)
        {
            this.EnqueueCommand(LaserPointerCommands.Command.STEP, null, step);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> SwitchOnAndMoveAsync(LaserPoint point)
        {
            if (point is null)
            {
                return false;
            }

            this.EnqueueCommand(LaserPointerCommands.Command.LASER_ON, point);
            this.EnqueueCommand(LaserPointerCommands.Command.MOVE, point);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
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

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        private bool ClearConcurrentQueue(ConcurrentQueue<string> concurrentQueure)
        {
            while (concurrentQueure.TryDequeue(out var sendMessage)) { }
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

        /// <summary>
        /// Send the messages int the queue, in the right order.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ExecuteCommandsAsync()
        {
            var result = false;
            try
            {
                this.ClearConcurrentQueue(this.messagesReceivedQueue);
                this.ClearConcurrentQueue(this.errorsQueue);

                var client = new TcpClient();
                NetworkStream stream = null;

                while (!this.messagesToBeSendQueue.IsEmpty)
                {
                    try
                    {
                        if (this.messagesToBeSendQueue.TryDequeue(out var sendMessage))
                        {
                            if (!client.Connected)
                            {
                                client.SendTimeout = this.tcpTimeout;
                                await client.ConnectAsync(this.IpAddress, this.Port).ConfigureAwait(true);
                                stream = client.GetStream();
                            }

                            var data = Encoding.ASCII.GetBytes(sendMessage);
                            stream = client.GetStream();
                            stream.ReadTimeout = this.tcpTimeout;
                            stream.Write(data, 0, data.Length);
                            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};LaserPointerDriver;ExecuteCommands();Sent: {sendMessage}");

                            if (this.IsWaitResponse(sendMessage))
                            {
                                data = new byte[client.ReceiveBufferSize];
                                var bytes = stream.Read(data, 0, data.Length);
                                var responseMessage = Encoding.ASCII.GetString(data, 0, bytes);

                                this.messagesReceivedQueue.Enqueue(responseMessage);
                                System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};LaserPointerDriver;ExecuteCommands();Received: {responseMessage}");
                                if (!this.IsResponseOk(sendMessage, responseMessage))
                                {
                                    System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss)};LaserPointerDriver;ExecuteCommands;ArgumentException;{sendMessage},{responseMessage}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss)};LaserPointerDriver;ExecuteCommands;ArgumentException;no wait {sendMessage}");
                                this.messagesReceivedQueue.Enqueue("");
                            }
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};LaserPointerDriver;ExecuteCommands;{e.Message}");
                        this.errorsQueue.Enqueue(e.Message);
                    }
                }

                stream?.Close();
                client?.Close();

                if (this.errorsQueue.Count == 0)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};LaserPointerDriver;{e.Message}");
            }

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
                if (msgReceive == "TEST ON OK" ||
                    msgReceive == "TEST OFF OK")
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
                if (msgReceive == LaserPointerCommands.OK)
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

            if (message.StartsWith("CLEAR", StringComparison.Ordinal) || message.StartsWith("ENABLE", StringComparison.Ordinal) || message.StartsWith("TEST OFF", StringComparison.Ordinal))
            {
                result = false;
            }

            return result;
        }

        #endregion
    }
}
