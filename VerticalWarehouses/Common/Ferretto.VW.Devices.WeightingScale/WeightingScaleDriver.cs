using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Ferretto.VW.Devices.WeightingScale
{
    public sealed class WeightingScaleDriver : IWeightingScaleDriver
    {
        #region Fields

        public const string IP_ADDRESS_DEFAULT = "192.168.0.14";

        public const int PORT_DEFAULT = 4001;

        private const string NEW_LINE = "\r\n";

        private readonly ConcurrentQueue<string> errorsQueue = new ConcurrentQueue<string>();

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentQueue<string> messagesReceivedQueue = new ConcurrentQueue<string>();

        private readonly ConcurrentQueue<string> messagesToBeSendQueue = new ConcurrentQueue<string>();

        private readonly int tcpTimeout = 1200;

        private TcpClient client;

        private IPAddress ipAddress = IPAddress.Parse(IP_ADDRESS_DEFAULT);

        private int port = PORT_DEFAULT;

        private NetworkStream stream = null;

        #endregion

        #region Properties

        public IPAddress IpAddress => this.ipAddress;

        public bool IsConnected => this.client?.Connected ?? false;

        public int Port => this.port;

        #endregion

        #region Methods

        public async Task ClearMessageAsync()
        {
            var displayIdentifier = 1;

            await this.SendCommandAsync($"DINT{displayIdentifier:00}0001");
        }

        public async Task ConnectAsync(IPAddress ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;

            try
            {
                if (this.client is null)
                {
                    this.client = new TcpClient();
                    this.stream = null;
                }
                if (!this.IsConnected)
                {
                    this.client.SendTimeout = this.tcpTimeout;
                    await this.client.ConnectAsync(this.IpAddress, this.Port).ConfigureAwait(true);
                    this.stream = this.client.GetStream();
                }
            }
            catch (Exception e)
            {
                this.logger.Error(e);
                this.Disconnect();
            }
        }

        public void Disconnect()
        {
            try
            {
                this.stream?.Close();
                this.client?.Close();
                this.client = null;
            }
            catch (Exception e)
            {
                this.logger.Error(e);
            }
        }

        public async Task DisplayMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("The message cannot be null or whitespace.", nameof(message));
            }

            var displayIdentifier = 1;
            await this.SendCommandAsync($"DINT{displayIdentifier:00}0000");
            await this.SendCommandAsync($"DISP{displayIdentifier:00}{message}");
        }

        public async Task DisplayMessageAsync(string message, TimeSpan duration)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("The message cannot be null or whitespace.", nameof(message));
            }

            var totalMilliseconds = duration.TotalMilliseconds;
            if (totalMilliseconds <= 0)
            {
                throw new ArgumentNullException("The duration must be strictly positive.", nameof(duration));
            }

            await this.SendCommandAsync($"DINT01{totalMilliseconds:X4}");
            await this.SendCommandAsync($"DISP01{message}");
        }

        public async Task<IWeightSample> MeasureWeightAsync()
        {
            WeightSample acquiredSample = null;
            var line = await this.SendCommandAsync($"REXT");

            if (!string.IsNullOrEmpty(line) && WeightSample.TryParse(line, out var sample))
            {
                acquiredSample = sample;
            }

            return acquiredSample;
        }

        public async Task ResetAverageUnitaryWeightAsync()
        {
            await this.SendCommandAsync($"SPMU0000.0");
        }

        public async Task<string> RetrieveVersionAsync()
        {
            string versionString = null;
            var line = await this.SendCommandAsync("VER");
            if (!string.IsNullOrEmpty(line))
            {
                versionString = line.Replace("VER,", string.Empty);
            }

            return versionString;
        }

        public async Task SetAverageUnitaryWeightAsync(float weight)
        {
            if (weight <= 0)
            {
                throw new ArgumentNullException(nameof(weight), "Average unitary weight must be strictly positive.");
            }

            var line = await this.SendCommandAsync($"SPMU{weight:0.0}");
        }

        private bool ClearConcurrentQueue(ConcurrentQueue<string> concurrentQueure)
        {
            while (concurrentQueure.TryDequeue(out var sendMessage)) { }
            return true;
        }

        private async Task<string> SendCommandAsync(string command)
        {
            this.logger.Debug($"sending command '{command}'.");
            this.ClearConcurrentQueue(this.messagesToBeSendQueue);
            this.messagesToBeSendQueue.Enqueue(command);
            this.ClearConcurrentQueue(this.messagesReceivedQueue);
            this.ClearConcurrentQueue(this.errorsQueue);

            if (!this.IsConnected)
            {
                await this.ConnectAsync(this.ipAddress, this.port);
            }

            if (this.IsConnected)
            {
                try
                {
                    //while (!this.messagesToBeSendQueue.IsEmpty)
                    //{
                    if (this.messagesToBeSendQueue.TryDequeue(out var sendMessage))
                    {
                        var data = Encoding.ASCII.GetBytes($"{sendMessage}{NEW_LINE}");
                        this.stream = this.client.GetStream();
                        this.stream.ReadTimeout = this.tcpTimeout;
                        this.stream.WriteTimeout = this.tcpTimeout;
                        this.stream.Write(data, 0, data.Length);
                        this.logger.Debug($"SendCommandAsync();Sent: {sendMessage.Replace("\r", "<CR>").Replace("\n", "<LF>")}");

                        data = new byte[this.client.ReceiveBufferSize];
                        var bytes = this.stream.Read(data, 0, data.Length);
                        var response = Encoding.ASCII.GetString(data, 0, bytes);

                        this.logger.Debug($"SendCommandAsync();Received: {response.Replace("\r", "<CR>").Replace("\n", "<LF>")}");

                        //switch (response)
                        //{
                        //    case "ERR01": throw new Exception($"The string '{command}' is a valid command but it is followed by unexpected characters.");
                        //    case "ERR02": throw new Exception($"The command '{command}' contains invalid data.");
                        //    case "ERR03": throw new Exception($"The command '{command}' is not valid in the current context.");
                        //    case "ERR04": throw new Exception($"The string '{command}' is not a valid command.");
                        //    default: return response;
                        //}

                        if (!response.Contains("ERR01") &&
                           !response.Contains("ERR02") &&
                           !response.Contains("ERR03") &&
                           !response.Contains("ERR04")
                            )
                        {
                            this.messagesReceivedQueue.Enqueue(response);
                        }
                        else
                        {
                            this.messagesReceivedQueue.Enqueue("");
                            this.errorsQueue.Enqueue(response);
                        }
                        //}
                        //else
                        //{
                        //    System.Threading.Thread.Sleep(100);
                        //}
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error(e);
                    this.Disconnect();
                }
            }
            if (this.messagesReceivedQueue.Count > 0)
            {
                this.messagesReceivedQueue.TryPeek(out var response);
                return response;
            }
            return string.Empty;
        }

        #endregion
    }
}
