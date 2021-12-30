using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Ferretto.VW.Devices.WeightingScale
{
    public sealed class WeightingScaleDriverMinebea : IWeightingScaleDriverMinebea
    {
        #region Fields

        public const string IP_ADDRESS_DEFAULT = "192.168.0.14";

        public const int PORT_DEFAULT = 4001;

        private const string ESC = "\x1b";

        private const string NEW_LINE = "\r\n";

        private static readonly System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(
            @"(?<Type>.{6})(?<Weight>-?\d+.\d+)\s*(?<UnitOfMeasure>\w)",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

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
            await this.SendCommandAsync($"t_");
        }

        public async Task ConnectAsync(IPAddress ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;

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
                    this.logger.Trace($"Connect");
                    this.client.SendTimeout = this.tcpTimeout;
                    await this.client.ConnectAsync(this.IpAddress, this.Port);
                    this.stream = this.client.GetStream();
                    this.logger.Debug($"Connected");
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
                this.stream?.Close();
                this.client?.Close();
                this.client = null;
                this._semaphore.Release();
                this.logger.Debug($"Disconnected");
            }
            catch (Exception e)
            {
                this._semaphore.Release();
                this.logger.Error(e);
            }
        }

        public async Task DisplayMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("The message cannot be null or whitespace.", nameof(message));
            }

            await this.SendCommandAsync($"t{message}_");
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

            await this.SendCommandAsync($"t{message}_");
            Thread.Sleep((int)totalMilliseconds);
        }

        public async Task<IWeightSample> MeasureWeightAsync()
        {
            WeightSampleMinebea acquiredSample = null;
            var line = await this.SendCommandAsync($"P");

            if (!string.IsNullOrEmpty(line) && WeightSampleMinebea.TryParse(line, out var sample))
            {
                acquiredSample = sample;
            }

            return acquiredSample;
        }

        public async Task ResetAverageUnitaryWeightAsync()
        {
            // await this.SendCommandAsync($"SPMU0000.0");
        }

        public async Task<string> RetrieveVersionAsync()
        {
            var versionString = await this.SendCommandAsync("i_");

            return versionString;
        }

        /// <summary>
        /// this function is not available in Minebea
        /// </summary>
        /// <param name="weight"></param>
        /// <returns></returns>
        public async Task SetAverageUnitaryWeightAsync(float weight)
        {
            if (weight < 0)
            {
                throw new ArgumentNullException(nameof(weight), "Average unitary weight must be positive or zero.");
            }
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

            await this._semaphore.WaitAsync();
            if (this.IsConnected)
            {
                try
                {
                    //while (!this.messagesToBeSendQueue.IsEmpty)
                    //{
                    if (this.messagesToBeSendQueue.TryDequeue(out var sendMessage))
                    {
                        var data = Encoding.ASCII.GetBytes($"{ESC}{sendMessage}{NEW_LINE}");
                        this.stream = this.client.GetStream();
                        this.stream.ReadTimeout = this.tcpTimeout;
                        this.stream.WriteTimeout = this.tcpTimeout;
                        this.stream.Write(data, 0, data.Length);
                        this.logger.Debug($"SendCommandAsync();Sent: <ESC>{sendMessage}<CR><LF>");

                        data = new byte[this.client.ReceiveBufferSize];
                        var bytes = this.stream.Read(data, 0, data.Length);
                        var response = Encoding.ASCII.GetString(data, 0, bytes);

                        this.logger.Debug($"SendCommandAsync();Received: {response.Replace(ESC, "<ESC>").Replace("\r", "<CR>").Replace("\n", "<LF>")}");

                        if (!response.Contains("Err") &&
                           !response.Contains("H") &&
                           !response.Contains("L")
                            )
                        {
                            this.messagesReceivedQueue.Enqueue(response);
                        }
                        else
                        {
                            this.messagesReceivedQueue.Enqueue("");
                            this.errorsQueue.Enqueue(response);
                        }
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
            else
            {
                this._semaphore.Release();
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
