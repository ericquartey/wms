using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.Simulator.Services.Interfaces;
using Ferretto.VW.Simulator.Services.Models;
using NLog;
using Prism.Mvvm;
using static Ferretto.VW.Simulator.Services.BufferUtility;

namespace Ferretto.VW.Simulator.Services
{
    internal class MachineService : BindableBase, IMachineService
    {
        #region Fields

        public byte[] Buffer;

        private const int DELAY_INVERTER_CLIENT = 5;

        private const int DELAY_IO_CLIENT = 5;

        private static readonly Random random = new Random();

        private readonly int DELAY_HEARTBEAT = 800;

        private readonly TcpListener listenerAlphaNumericBar1 = new TcpListener(IPAddress.Any, 2020);

        private readonly TcpListener listenerInverter = new TcpListener(IPAddress.Any, 17221);

        private readonly TcpListener listenerIoDriver1 = new TcpListener(IPAddress.Any, 19550);

        private readonly TcpListener listenerIoDriver2 = new TcpListener(IPAddress.Any, 19551);

        private readonly TcpListener listenerIoDriver3 = new TcpListener(IPAddress.Any, 19552);

        private readonly TcpListener listenerLaser1 = new TcpListener(IPAddress.Any, 12020);

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ObservableCollection<IODeviceModel> remoteIOs = new ObservableCollection<IODeviceModel>();

        private string alphaNumericBar1Message = "";

        private int alphaNumericBar1Offset = 0;

        private CancellationTokenSource cts = new CancellationTokenSource();

        private DateTime heartBeatTime;

        private bool isInverterConnected;

        private Machine machine;

        #endregion

        #region Constructors

        public MachineService()
        {
            this.heartBeatTime = DateTime.UtcNow;
            this.remoteIOs.Add(new IODeviceModel() { Id = 0 });
            this.remoteIOs.Add(new IODeviceModel() { Id = 1 });
            this.remoteIOs.Add(new IODeviceModel() { Id = 2 });

            this.Inverters = new ObservableCollection<InverterModel>();
            this.Inverters.Add(new InverterModel(Models.InverterType.Ang) { ioDeviceMain = this.remoteIOs[0].Inputs.ToArray(), ioDeviceBay = this.remoteIOs[0].Inputs.ToArray(), Id = 0, ImpulsesEncoderPerRound = 77.61182 });
            this.Inverters.Add(new InverterModel(Models.InverterType.Acu) { ioDeviceMain = this.remoteIOs[0].Inputs.ToArray(), ioDeviceBay = this.remoteIOs[0].Inputs.ToArray(), Id = 1, ImpulsesEncoderPerRound = 77.6722 });
            this.Inverters.Add(new InverterModel(Models.InverterType.Agl) { ioDeviceMain = this.remoteIOs[0].Inputs.ToArray(), ioDeviceBay = this.remoteIOs[0].Inputs.ToArray(), Id = 2 });
            this.Inverters.Add(new InverterModel(Models.InverterType.Acu) { ioDeviceMain = this.remoteIOs[0].Inputs.ToArray(), ioDeviceBay = this.remoteIOs[0].Inputs.ToArray(), Id = 3, ImpulsesEncoderPerRound = 369.8453 });
            this.Inverters.Add(new InverterModel(Models.InverterType.Agl) { ioDeviceMain = this.remoteIOs[0].Inputs.ToArray(), ioDeviceBay = this.remoteIOs[1].Inputs.ToArray(), Id = 4 });
            this.Inverters.Add(new InverterModel(Models.InverterType.Acu) { ioDeviceMain = this.remoteIOs[0].Inputs.ToArray(), ioDeviceBay = this.remoteIOs[1].Inputs.ToArray(), Id = 5, ImpulsesEncoderPerRound = 369.8453 });
            this.Inverters.Add(new InverterModel(Models.InverterType.Agl) { ioDeviceMain = this.remoteIOs[0].Inputs.ToArray(), ioDeviceBay = this.remoteIOs[2].Inputs.ToArray(), Id = 6 });
            this.Inverters.Add(new InverterModel(Models.InverterType.Acu) { ioDeviceMain = this.remoteIOs[0].Inputs.ToArray(), ioDeviceBay = this.remoteIOs[2].Inputs.ToArray(), Id = 7 });

            this.Inverters00.OnHorizontalMovementComplete += this.OnHorizontalMovementCompleted;
            this.Inverters01.OnHorizontalMovementComplete += this.OnHorizontalMovementCompleted;
            this.Inverters03.OnHorizontalMovementComplete += this.OnHorizontalMovementCompleted;
            this.Inverters05.OnHorizontalMovementComplete += this.OnHorizontalMovementCompleted;
            this.Inverters07.OnHorizontalMovementComplete += this.OnHorizontalMovementCompleted;

            this.MinTorqueCurrent = 72;
            this.MaxTorqueCurrent = 120;

            this.MinProfileHeight = new Dictionary<LoadingUnitLocation, int>();
            this.MaxProfileHeight = new Dictionary<LoadingUnitLocation, int>();
            this.MinProfileHeight.Add(LoadingUnitLocation.NoLocation, 2000);
            this.MaxProfileHeight.Add(LoadingUnitLocation.NoLocation, 10000);
        }

        #endregion

        #region Properties

        public ObservableCollection<InverterModel> Inverters { get; set; }

        public InverterModel Inverters00
        { get => this.Inverters[0]; set { var inv = this.Inverters[0]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters01
        { get => this.Inverters[1]; set { var inv = this.Inverters[1]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters02
        { get => this.Inverters[2]; set { var inv = this.Inverters[2]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters03
        { get => this.Inverters[3]; set { var inv = this.Inverters[3]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters04
        { get => this.Inverters[4]; set { var inv = this.Inverters[4]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters05
        { get => this.Inverters[5]; set { var inv = this.Inverters[5]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters06
        { get => this.Inverters[6]; set { var inv = this.Inverters[6]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters07
        { get => this.Inverters[7]; set { var inv = this.Inverters[7]; this.SetProperty(ref inv, value); } }

        public bool IsErrorGeneratorInverter { get; set; }

        public bool IsErrorGeneratorIODevice { get; set; }

        public bool IsStartedSimulator { get; private set; }

        public Machine Machine
        {
            get => this.machine;
            set
            {
                this.machine = value;
                this.remoteIOs.ForEach(x => x.Machine = value);
                this.Inverters.ForEach(x => x.Machine = value);

                foreach (var inverter in this.Inverters)
                {
                    switch (inverter.InverterType)
                    {
                        case Models.InverterType.Ang:
                            //x inverter.DigitalIO[(int)InverterSensors.ANG_ZeroElevatorSensor].Value = false;
                            break;

                        case Models.InverterType.Acu:
                            inverter.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!inverter.IsExternal) ? true : false;
                            inverter.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = !inverter.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value;
                            break;

                        case Models.InverterType.Agl:
                            break;

                        default:
                            break;
                    }
                }

                if (this.machine.LoadUnitMaxNetWeight > 800)
                {
                    this.MinTorqueCurrent = 69;
                    this.MaxTorqueCurrent = 127;
                }
                else if (this.machine.LoadUnitMaxNetWeight > 550)
                {
                    this.MinTorqueCurrent = 74;
                    this.MaxTorqueCurrent = 138;
                }
                else if (this.machine.LoadUnitMaxNetWeight > 300)
                {
                    this.MinTorqueCurrent = 70;
                    this.MaxTorqueCurrent = 112;
                }
                else
                {
                    this.MinTorqueCurrent = 90;
                    this.MaxTorqueCurrent = 128;
                }
                // simulate weight errors
                //this.MaxTorqueCurrent = (int)(this.MaxTorqueCurrent * 1.1);

                this.MinProfileHeight.Clear();
                this.MaxProfileHeight.Clear();
                this.MinProfileHeight.Add(LoadingUnitLocation.NoLocation, 2000);
                this.MaxProfileHeight.Add(LoadingUnitLocation.NoLocation, 10000);
                foreach (var bayPosition in this.machine.Bays.Where(b => b.Positions != null).SelectMany(p => p.Positions))
                {
                    this.MinProfileHeight.Add(bayPosition.Location, (int)Math.Round((this.machine.LoadUnitMinHeight - bayPosition.ProfileOffset + 212.5) / 0.0938));
                    var maxHeight = (bayPosition.MaxDoubleHeight > 0) ? bayPosition.MaxDoubleHeight : bayPosition.MaxSingleHeight;
                    if (maxHeight > this.machine.LoadUnitMaxHeight)
                    {
                        Debug.WriteLine($"{DateTime.Now}: Warning: configuration error! max bay height {maxHeight} > LoadUnitMaxHeight {this.machine.LoadUnitMaxHeight}");
                        maxHeight = this.machine.LoadUnitMaxHeight;
                    }
                    // simulate height errors
                    //maxHeight *= 1.1;

                    this.MaxProfileHeight.Add(bayPosition.Location, (int)Math.Round((maxHeight - bayPosition.ProfileOffset + 212.5) / 0.0938));
                    if (this.MaxProfileHeight[bayPosition.Location] > this.MaxProfileHeight[LoadingUnitLocation.NoLocation])
                    {
                        // warning: configuration error!!!
                        Debug.WriteLine($"{DateTime.Now}: Warning: configuration error! max profile height {this.MaxProfileHeight[bayPosition.Location]} > 10000");
                        this.MaxProfileHeight[bayPosition.Location] = this.MaxProfileHeight[LoadingUnitLocation.NoLocation];
                    }
                }
            }
        }

        public Dictionary<LoadingUnitLocation, int> MaxProfileHeight { get; private set; }

        public int MaxTorqueCurrent { get; private set; }

        public Dictionary<LoadingUnitLocation, int> MinProfileHeight { get; private set; }

        public int MinTorqueCurrent { get; private set; }

        public IODeviceModel RemoteIOs01
        { get => this.remoteIOs[0]; set { var ios = this.remoteIOs[0]; this.SetProperty(ref ios, value); } }

        public IODeviceModel RemoteIOs02
        { get => this.remoteIOs[1]; set { var ios = this.remoteIOs[1]; this.SetProperty(ref ios, value); } }

        public IODeviceModel RemoteIOs03
        { get => this.remoteIOs[2]; set { var ios = this.remoteIOs[2]; this.SetProperty(ref ios, value); } }

        #endregion

        #region Methods

        public async Task ProcessErrorGeneratorInverterAsync()
        {
            this.IsErrorGeneratorInverter = !this.IsErrorGeneratorInverter;
            this.Logger.Trace($"ErrorGeneratorInverter: {this.IsErrorGeneratorInverter}");

            this.RaisePropertyChanged(nameof(this.IsErrorGeneratorInverter));
        }

        public async Task ProcessErrorGeneratorIODeviceAsync()
        {
            this.IsErrorGeneratorIODevice = !this.IsErrorGeneratorIODevice;
            this.Logger.Trace($"ErrorGeneratorIODevice: {this.IsErrorGeneratorIODevice}");

            this.RaisePropertyChanged(nameof(this.IsErrorGeneratorIODevice));
        }

        /// <summary>
        /// Start simulator, inizialize socket (???)
        /// </summary>
        /// <returns></returns>
        public async Task ProcessStartSimulatorAsync()
        {
            this.Logger.Trace("1:ProcessStartSimulator");

            this.cts = new CancellationTokenSource();
            this.listenerInverter.Start();
            if (this.RemoteIOs01.Enabled)
            {
                this.listenerIoDriver1.Start();
            }

            if (this.RemoteIOs02.Enabled)
            {
                this.listenerIoDriver2.Start();
            }

            if (this.RemoteIOs03.Enabled)
            {
                this.listenerIoDriver3.Start();
            }

            this.listenerLaser1.Start();

            this.listenerAlphaNumericBar1.Start();

            _ = Task.Run(() => this.AcceptClient(this.listenerInverter, this.cts.Token, (client, message) => this.ReplyInverter(client, message)));
            if (this.RemoteIOs01.Enabled)
            {
                _ = Task.Run(() => this.AcceptClient(this.listenerIoDriver1, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message, 0)));
            }

            if (this.RemoteIOs02.Enabled)
            {
                _ = Task.Run(() => this.AcceptClient(this.listenerIoDriver2, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message, 1)));
            }

            if (this.RemoteIOs03.Enabled)
            {
                _ = Task.Run(() => this.AcceptClient(this.listenerIoDriver3, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message, 2)));
            }

            _ = Task.Run(() => this.AcceptClient(this.listenerLaser1, this.cts.Token, (client, message) => this.ReplyLaser(client, message, 1)));

            _ = Task.Run(() => this.AcceptClient(this.listenerAlphaNumericBar1, this.cts.Token, (client, message) => this.ReplyAlphaNumericBar1(client, message, 1)));

            await Task.Delay(100);
            this.IsStartedSimulator = true;

            if (this.RemoteIOs01.Enabled)
            {
                this.RaisePropertyChanged(nameof(this.RemoteIOs01));
            }

            if (this.RemoteIOs02.Enabled)
            {
                this.RaisePropertyChanged(nameof(this.RemoteIOs02));
            }

            if (this.RemoteIOs03.Enabled)
            {
                this.RaisePropertyChanged(nameof(this.RemoteIOs03));
            }

            this.RaisePropertyChanged(nameof(this.IsStartedSimulator));
        }

        public async Task ProcessStopSimulatorAsync()
        {
            this.Logger.Trace("1:ProcessStopSimulator");

            this.cts.Cancel();
            this.listenerInverter.Stop();

            if (this.RemoteIOs01.Enabled)
            {
                this.listenerIoDriver1.Stop();
            }

            if (this.RemoteIOs02.Enabled)
            {
                this.listenerIoDriver2.Stop();
            }

            if (this.RemoteIOs03.Enabled)
            {
                this.listenerIoDriver3.Stop();
            }

            await Task.Delay(100);
            this.IsStartedSimulator = false;

            this.RaisePropertyChanged(nameof(this.IsStartedSimulator));
        }

        private void AcceptClient(TcpListener listener, CancellationToken token, Action<TcpClient, byte[]> messageHandler)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var client = listener.AcceptTcpClient();
                    Task.Run(() => this.ManageClient(client, this.cts.Token, messageHandler));
                }
            }
            catch (SocketException)
            {
            }
        }

        private byte[] FormatMessage(byte[] message, InverterRole systemIndex, byte dataSetIndex, byte[] inputValues, bool isError = false)
        {
            int byteLength;
            byte[] byteMessage;
            byteLength = 0x04 + inputValues.Length;
            byteMessage = new byte[byteLength + 2];
            byteMessage[0] = (byte)(isError ? (message[0] | 0x40) : 0x00);
            byteMessage[1] = (byte)byteLength;
            byteMessage[2] = (byte)systemIndex;
            byteMessage[3] = dataSetIndex;
            byteMessage[4] = message[4];
            byteMessage[5] = message[5];
            Array.Copy(inputValues, 0, byteMessage, 6, inputValues.Length);
            return byteMessage;
        }

        private void GetProfileRange(InverterModel inverter, out int minProfileHeight, out int maxProfileHeight)
        {
            minProfileHeight = this.MinProfileHeight[LoadingUnitLocation.NoLocation];
            maxProfileHeight = this.MaxProfileHeight[LoadingUnitLocation.NoLocation];
            if (this.Machine != null)
            {
                var bays = this.Machine.Bays.Where(b => b.Positions != null);
                if (bays.Any())
                {
                    Bay bay = null;
                    switch (inverter.InverterRole)
                    {
                        case InverterRole.Main:
                            bay = bays.FirstOrDefault(b => b.Number == BayNumber.BayOne);
                            break;

                        case InverterRole.Shutter2:
                            bay = bays.FirstOrDefault(b => b.Number == BayNumber.BayTwo);
                            break;

                        case InverterRole.Shutter3:
                            bay = bays.FirstOrDefault(b => b.Number == BayNumber.BayThree);
                            break;

                        default:
                            return;
                    }
                    if (bay != null)
                    {
                        var bayPosition = bay.Positions.FirstOrDefault(x => Math.Abs(x.Height - this.Inverters00.AxisPositionY - this.Machine.Elevator.Axes.First().Offset) <= 5);
                        if (bayPosition != null)
                        {
                            this.MinProfileHeight.TryGetValue(bayPosition.Location, out minProfileHeight);
                            this.MaxProfileHeight.TryGetValue(bayPosition.Location, out maxProfileHeight);
                        }
                    }
                }
            }
        }

        private void ManageClient(TcpClient client, CancellationToken token, Action<TcpClient, byte[]> messageHandler)
        {
            using (client)
            {
                var lastReceivedMessage = DateTime.UtcNow;
                var buffer = new byte[1024];
                var socket = client.Client;
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (socket != null && socket.Connected)
                        {
                            if (socket.Poll(50000, SelectMode.SelectRead))
                            {
                                lastReceivedMessage = DateTime.UtcNow;
                                var bytes = socket.Receive(buffer);
                                if (bytes > 0)
                                {
                                    var message = new byte[bytes];
                                    Array.Copy(buffer, 0, message, 0, message.Length);
                                    messageHandler(client, message);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else if (DateTime.UtcNow.Subtract(lastReceivedMessage).TotalSeconds >= 20)
                            {
                                //client.Close();
                                //break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (SocketException)
                {
                }
            }
        }

        private void OnHorizontalMovementCompleted(object sender, HorizontalMovementEventArgs e)
        {
            if (this.Machine == null)
            {
                return;
            }

            // Check if movement was executed on a bay (use coordinate to determine position)
            var bays = this.Machine.Bays.Where(b => b.Positions != null);
            if (bays.Any())
            {
                foreach (var bay in bays)
                {
                    // check if shutter is closed
                    var isShutterClosed = false;
                    if (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified)
                    {
                        switch ((int)bay.Shutter.Inverter.Index)
                        {
                            case 2:
                                isShutterClosed = !this.Inverters02.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value && !this.Inverters02.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value;
                                break;

                            case 4:
                                isShutterClosed = !this.Inverters04.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value && !this.Inverters04.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value;
                                break;

                            case 6:
                                isShutterClosed = !this.Inverters06.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value && !this.Inverters06.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value;
                                break;
                        }
                    }
                    if (isShutterClosed)
                    {
                        continue;
                    }
                    var isCarousel = bay.Carousel != null;
                    var isExternal = bay.IsExternal;
                    var isDouble = bay.IsDouble;
                    var inverter = sender as InverterModel;

                    // Retrieve bay position (upper/lower position)
                    var bayPosition = bay.Positions.FirstOrDefault(x => Math.Abs(x.Height - this.Inverters00.AxisPositionY - this.Machine.Elevator.Axes.First().Offset) <= 8);

                    if (bayPosition != null)
                    {
                        // Set/Reset bay presence
                        switch (bay.Number)
                        {
                            case BayNumber.BayOne:
                                if (bayPosition.IsUpper)
                                {
                                    //this.RemoteIOs01.Inputs[(int)IoPorts.LoadingUnitInBay].Value = e.IsLoading;

                                    if (!isExternal)
                                    {
                                        this.RemoteIOs01.Inputs[(int)IoPorts.LoadingUnitInBay].Value = e.IsLoading;
                                    }
                                    else if (isDouble && isExternal)
                                    {
                                        this.RemoteIOs01.Inputs[(int)IoPorts.HookTrolley].Value = e.IsLoading;
                                    }
                                    else
                                    {
                                        // check state of external sensor for the ext bay
                                        if (this.RemoteIOs01.Inputs[(int)IoPorts.LoadingUnitInBay].Value)
                                        {
                                            this.RemoteIOs01.Inputs[(int)IoPorts.LoadingUnitInLowerBay].Value = !e.IsLoading;
                                        }
                                    }
                                }
                                else
                                {
                                    if (isDouble && isExternal)
                                    {
                                        this.RemoteIOs01.Inputs[(int)IoPorts.FinePickingRobot].Value = e.IsLoading;
                                    }
                                    else
                                    {
                                        this.RemoteIOs01.Inputs[(int)IoPorts.LoadingUnitInLowerBay].Value = (isCarousel ? !e.IsLoading : e.IsLoading);
                                    }
                                }
                                break;

                            case BayNumber.BayTwo:
                                if (bayPosition.IsUpper)
                                {
                                    //this.RemoteIOs02.Inputs[(int)IoPorts.LoadingUnitInBay].Value = e.IsLoading;

                                    if (!isExternal)
                                    {
                                        this.RemoteIOs02.Inputs[(int)IoPorts.LoadingUnitInBay].Value = e.IsLoading;
                                    }
                                    else if (isDouble && isExternal)
                                    {
                                        this.RemoteIOs02.Inputs[(int)IoPorts.HookTrolley].Value = e.IsLoading;
                                    }
                                    else
                                    {
                                        if (this.RemoteIOs02.Inputs[(int)IoPorts.LoadingUnitInBay].Value)
                                        {
                                            this.RemoteIOs02.Inputs[(int)IoPorts.LoadingUnitInLowerBay].Value = !e.IsLoading;
                                        }
                                    }
                                }
                                else
                                {
                                    if (isDouble && isExternal)
                                    {
                                        this.RemoteIOs02.Inputs[(int)IoPorts.FinePickingRobot].Value = e.IsLoading;
                                    }
                                    else
                                    {
                                        this.RemoteIOs02.Inputs[(int)IoPorts.LoadingUnitInLowerBay].Value = (isCarousel ? !e.IsLoading : e.IsLoading);
                                    }
                                }
                                break;

                            case BayNumber.BayThree:
                                if (bayPosition.IsUpper)
                                {
                                    //this.RemoteIOs03.Inputs[(int)IoPorts.LoadingUnitInBay].Value = e.IsLoading;

                                    if (!isExternal)
                                    {
                                        this.RemoteIOs03.Inputs[(int)IoPorts.LoadingUnitInBay].Value = e.IsLoading;
                                    }
                                    else if (isDouble && isExternal)
                                    {
                                        this.RemoteIOs03.Inputs[(int)IoPorts.HookTrolley].Value = e.IsLoading;
                                    }
                                    else
                                    {
                                        if (this.RemoteIOs03.Inputs[(int)IoPorts.LoadingUnitInBay].Value)
                                        {
                                            this.RemoteIOs03.Inputs[(int)IoPorts.LoadingUnitInLowerBay].Value = !e.IsLoading;
                                        }
                                    }
                                }
                                else
                                {
                                    if (isDouble && isExternal)
                                    {
                                        this.RemoteIOs03.Inputs[(int)IoPorts.FinePickingRobot].Value = e.IsLoading;
                                    }
                                    else
                                    {
                                        this.RemoteIOs03.Inputs[(int)IoPorts.LoadingUnitInLowerBay].Value = (isCarousel ? !e.IsLoading : e.IsLoading);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void ReplyAlphaNumericBar1(TcpClient client, byte[] message, int index)
        {
            var messageReceived = Encoding.ASCII.GetString(message).Trim();
            var messageResponse = "OK";

            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss} ReplyAlphaNumericBar1 receive <-- '{messageReceived}'");

            if (messageReceived.StartsWith("TEST ON", StringComparison.Ordinal))
            {
                messageResponse = "TEST ON OK";
            }
            else if (messageReceived.StartsWith("TEST OFF", StringComparison.Ordinal))
            {
                messageResponse = "TEST OFF OK";
            }
            else if (messageReceived.StartsWith("SET ", StringComparison.Ordinal)
                || messageReceived.StartsWith("SCROLL ON ", StringComparison.Ordinal))
            {
                var subStr = messageReceived.Split(' ');
                if (subStr.Length > 1)
                {
                    this.alphaNumericBar1Offset = int.Parse(subStr[1]);
                    this.alphaNumericBar1Message = messageReceived.Substring(messageReceived.IndexOf(subStr[1], StringComparison.Ordinal) + subStr[1].Length).Trim();
                }
            }
            else if (messageReceived.StartsWith("GET", StringComparison.Ordinal))
            {
                messageResponse = "GET " + this.alphaNumericBar1Message;
            }
            else if (messageReceived.StartsWith("TEST OFF", StringComparison.Ordinal))
            {
                return;
            }

            Task.Delay(100).Wait();
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss} ReplyAlphaNumericBar1 response --> '{messageResponse}'");
            client.Client.Send(Encoding.ASCII.GetBytes(messageResponse + Environment.NewLine));
        }

        private void ReplyInverter(TcpClient client, byte[] incomingBytes)
        {
            const int headerLenght = 6;
            this.Buffer = this.Buffer.AppendArrays(incomingBytes, incomingBytes.Length);
            if (this.Buffer.Length >= headerLenght && this.Buffer.Length >= this.Buffer[1] + 2)
            {
                var extractedMessages = GetMessagesWithHeaderLengthToEnqueue(ref this.Buffer, 4, 1, 2);

                foreach (var extractedMessage in extractedMessages)
                {
                    var message = InverterMessage.FromBytes(extractedMessage);

                    if (this.IsErrorGeneratorInverter
                        && (ushort)random.Next(10000) == 50)
                    {
                        this.Logger.Debug($"Generate Inverter error");
                    }
                    else
                    {
                        this.ReplyToInverterMessage(client, message);
                    }

                    Thread.Sleep(DELAY_INVERTER_CLIENT * random.Next(1, 10));
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void ReplyIoDriver(TcpClient client, byte[] message, int index)
        {
            const int NBYTES_RECEIVE = 15;
            const int NBYTES_RECEIVE_CFG = 3;

            var device = this.remoteIOs.First(x => x.Id == index);

            device.Buffer = device.Buffer.AppendArrays(message, message.Length);
            if (device.Buffer.Length > 2 && device.Buffer.Length >= device.Buffer[0])
            {
                var extractedMessages = GetMessagesWithHeaderLengthToEnqueue(ref device.Buffer, 3, 0, 0);
                foreach (var extractedMessage in extractedMessages)
                {
                    var length = extractedMessage[0];
                    var firmwareProtocol = extractedMessage[1];
                    var codeOperation = extractedMessage[2];
                    var outputs = (from x in Enumerable.Range(0, 8)
                                   let binary = Convert.ToString(device.FirmwareVersion == 0x10 ? extractedMessage[3] : extractedMessage[4], 2).PadLeft(8, '0')
                                   select new { Value = binary[x] == '1' ? true : false, Description = (7 - x).ToString(), Index = (7 - x) }).Reverse().ToArray();
                    for (var i = 0; i < outputs.Length; i++)
                    {
                        device.Outputs[i].Value = outputs[i].Value;
                        // TEST
                        device.OutDiagFail[i].Value = outputs[i].Value;
                        device.OutDiagCurrent[i] = outputs[i].Value ? 300 : 0;
                    }

                    byte[] responseMessage = null;
                    switch (codeOperation)
                    {
                        case 0x00: // Data
                            responseMessage = new byte[NBYTES_RECEIVE + (device.FirmwareVersion == 0x11 ? 11 : 0)];
                            responseMessage[0] = (byte)responseMessage.Length;  // nBytes
                            responseMessage[1] = device.FirmwareVersion;        // fwRelease
                            responseMessage[2] = 0x00;                          // Code op   0x00: data, 0x06: configuration
                            responseMessage[3] = 0x00;                          // error code
                            Array.Copy(extractedMessage, device.FirmwareVersion == 0x11 ? 4 : 3, responseMessage, device.FirmwareVersion == 0x11 ? 5 : 4, 1);  // output values echo
                            var inputs = BitConverter.GetBytes(device.InputsValue);
                            responseMessage[device.FirmwareVersion == 0x11 ? 6 : 5] = inputs[0];
                            responseMessage[device.FirmwareVersion == 0x11 ? 7 : 6] = inputs[1];
                            if (device.FirmwareVersion == 0x11)
                            {
                                for (var i = 0; i < device.OutDiagCurrent.Count; i++)
                                {
                                    var bytes = BitConverter.GetBytes(device.OutDiagCurrent[i]);
                                    responseMessage[8 + (i * 2)] = bytes[0];
                                    responseMessage[8 + (i * 2) + 1] = bytes[1];
                                }
                                responseMessage[24] = device.OutDiagFailValue;
                            }
                            break;

                        case 0x01: // Config
                            responseMessage = new byte[NBYTES_RECEIVE_CFG];
                            responseMessage[0] = NBYTES_RECEIVE_CFG;        // nBytes
                            responseMessage[1] = device.FirmwareVersion;    // fwRelease
                            responseMessage[2] = 0x06;                      // Ack  0x00: data, 0x06: configuration
                            break;

                        case 0x02: // SetIP
                            break;

                        default:
                            if (System.Diagnostics.Debugger.IsAttached)
                            {
                                System.Diagnostics.Debugger.Break();
                            }
                            break;
                    }

                    this.UpdateRemoteIO(device);

                    if (this.IsErrorGeneratorIODevice
                        && (ushort)random.Next(10000) == 50)
                    {
                        this.Logger.Debug($"Generate I/O error, index: {index}");
                    }
                    else
                    {
                        var result = client.Client.Send(responseMessage);
                    }
                    Thread.Sleep(DELAY_IO_CLIENT);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void ReplyLaser(TcpClient client, byte[] message, int index)
        {
            var messageText = Encoding.ASCII.GetString(message).Trim();
            if (messageText.StartsWith("TEST ON", StringComparison.Ordinal)
                || messageText.StartsWith("TEST OFF", StringComparison.Ordinal)
                || messageText.StartsWith("LASER ON", StringComparison.Ordinal)
                || messageText.StartsWith("LASER OFF", StringComparison.Ordinal)
                || messageText.StartsWith("MOVE", StringComparison.Ordinal)
                || messageText.StartsWith("HOME", StringComparison.Ordinal)
                )
            {
                System.Diagnostics.Debug.WriteLine($"Simulation *** {messageText}");
                client.Client.Send(Encoding.ASCII.GetBytes("OK\r\n"));
            }
        }

        private void ReplyToInverterMessage(TcpClient client, InverterMessage message)
        {
            var inverter = this.Inverters.First(x => x.InverterRole == (InverterRole)message.SystemIndex);
            if (!inverter.Enabled)
            {
                System.Diagnostics.Debug.WriteLine($"NOT ENABLED: {message}");
                return;
            }
            if (!this.isInverterConnected)
            {
                this.heartBeatTime = DateTime.UtcNow;
            }
            this.isInverterConnected = true;

            //if (DateTime.UtcNow.Subtract(this.heartBeatTime).TotalMilliseconds > this.DELAY_HEARTBEAT)
            //{
            //    var timeout = DateTime.UtcNow.Subtract(this.heartBeatTime).TotalMilliseconds;
            //    Debug.WriteLine($"{DateTime.Now}: HeartBeat timeout {timeout}");
            //    this.Logger.Error($"HeartBeat timeout {timeout}");

            //    // TODO: enable heartbeat control
            //    //inverter.IsFault = true;
            //    //if (Debugger.IsAttached)
            //    //{
            //    //    Debugger.Break();
            //    //}
            //}

            var result = 0;
            switch (message.ParameterId)
            {
                case InverterParameterId.HeartBeatTimer1:
                    result = client.Client.Send(message.ToBytes());
                    this.heartBeatTime = DateTime.UtcNow;
                    break;

                case InverterParameterId.ControlWord:
                    inverter.ControlWord = message.UShortPayload;
                    inverter.RefreshControlWordArray();
                    this.UpdateInverter(inverter);
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.DigitalInputsOutputs:
                    var values = this.Inverters.GroupBy(x => x.Id / 2).Select(x => x.First().GetDigitalIO() + (x.Last().GetDigitalIO() << 8)).ToArray();
                    var inputValues = $" {string.Join(" ", values)} ";
                    var ioStatusMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, Encoding.ASCII.GetBytes(inputValues));
                    result = client.Client.Send(ioStatusMessage);
                    break;

                case InverterParameterId.SetOperatingMode:
                    inverter.AxisChanged = 0;
                    inverter.OperationMode = (InverterOperationMode)message.UShortPayload;
                    if (inverter.OperationMode == InverterOperationMode.ProfileVelocity)
                    {
                        var errorResponse = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)17), true);
                        result = client.Client.Send(errorResponse);
                    }
                    else
                    {
                        result = client.Client.Send(message.ToBytes());
                    }
                    break;

                case InverterParameterId.HomingCalibration:
                    inverter.CalibrationMode = (InverterCalibrationMode)message.UShortPayload;
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.HomingCreepSpeed:
                case InverterParameterId.HomingFastSpeed:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.HomingSensor:
                case InverterParameterId.HomingOffset:
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.PositionAcceleration:
                    inverter.TargetAcceleration[inverter.CurrentAxis] = (int)message.UIntPayload;
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.PositionDeceleration:
                    inverter.TargetDeceleration[inverter.CurrentAxis] = (int)message.UIntPayload;
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.RMSCurrent:
                    // simulate measure weight
                    var torqueMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)random.Next(this.MinTorqueCurrent, this.MaxTorqueCurrent)));
                    //var torqueMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)((this.MinTorqueCurrent+ this.MaxTorqueCurrent)/2)));

                    // To simulate an overweight condition: value to use
                    // torqueMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)(this.MaxTorqueCurrent + 5)));

                    result = client.Client.Send(torqueMessage);
                    break;

                case InverterParameterId.ProfileInput:
                    // simulate measure profile height: random value inside the range
                    this.GetProfileRange(inverter, out var minProfileHeight, out var maxProfileHeight);
                    var profileMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)random.Next(minProfileHeight, maxProfileHeight)));

                    // TEST: always the same height
                    //profileMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)(3500))); // 174mm

                    // this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)(maxProfileHeight + 500)));

                    //
                    // TEST: check intrusion
                    //var profileMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)3918));
                    //

                    result = client.Client.Send(profileMessage);
                    break;

                case InverterParameterId.PositionTargetPosition:
                    inverter.TargetPosition[inverter.CurrentAxis] = inverter.Impulses2millimeters((int)message.UIntPayload);
                    inverter.StartPosition[inverter.CurrentAxis] = inverter.AxisPosition;
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.PositionTargetSpeed:
                    inverter.TargetSpeed[inverter.CurrentAxis] = (int)message.UIntPayload;
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.ShutterTargetVelocity:
                    inverter.SpeedRate = (int)message.UIntPayload;
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.StatusWord:
                    this.UpdateInverter(inverter);
                    switch (inverter.OperationMode)
                    {
                        case InverterOperationMode.Homing:
                            inverter.BuildHomingStatusWord();
                            break;

                        case InverterOperationMode.Position:
                            inverter.BuildPositionStatusWord();
                            break;

                        case InverterOperationMode.Velocity:
                        case InverterOperationMode.ProfileVelocity:
                            inverter.BuildVelocityStatusWord();
                            break;

                        case InverterOperationMode.TableTravel:
                            inverter.BuildTableTravelStatusWord();
                            break;

                        default:
                            if (System.Diagnostics.Debugger.IsAttached)
                            {
                                System.Diagnostics.Debugger.Break();
                            }
                            break;
                    }
                    var statusWordMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)inverter.StatusWord));
                    result = client.Client.Send(statusWordMessage);
                    break;

                case InverterParameterId.ActualPositionShaft:
                    var impulses = inverter.Millimeters2Impulses(inverter.AxisPosition);
                    var actualPositionMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes(impulses));
                    result = client.Client.Send(actualPositionMessage);
                    break;

                case InverterParameterId.StatusDigitalSignals:
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.ShutterTargetPosition:
                    inverter.TargetShutterPosition = message.UShortPayload;
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.TableTravelTargetPosition:
                    if (inverter.TableIndex < (int)InverterTableIndex.TableTravelP1)
                    {
                        inverter.TargetPosition[inverter.CurrentAxis] = inverter.Impulses2millimeters((int)message.UIntPayload);
                        inverter.StartPosition[inverter.CurrentAxis] = inverter.AxisPosition;
                        inverter.IsStartedOnBoard = this.remoteIOs[0].Inputs[(int)IoPorts.DrawerInMachineSide].Value
                            && this.remoteIOs[0].Inputs[(int)IoPorts.DrawerInOperatorSide].Value;
                    }
                    else
                    {
                        var switchId = inverter.TableIndex - (int)InverterTableIndex.TableTravelP1;
                        inverter.SwitchPositions[inverter.CurrentAxis][switchId] = inverter.Impulses2millimeters((int)message.UIntPayload);
                    }
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.TableTravelTargetSpeeds:
                    inverter.TargetSpeed[inverter.CurrentAxis] = Math.Max((int)message.UIntPayload, inverter.TargetSpeed[inverter.CurrentAxis]);
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.TableTravelTargetAccelerations:
                    {
                        var replyMessage = message.ToBytes();
                        if (message.UIntPayload == 0)
                        {
                            replyMessage = this.FormatMessage(replyMessage, (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)1), true);
                        }

                        result = client.Client.Send(replyMessage);
                    }
                    break;

                case InverterParameterId.TableTravelTargetDecelerations:
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.TableTravelTableIndex:
                    inverter.TableIndex = (short)message.UShortPayload;
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.TableTravelDirection:
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.ShutterLowVelocity:
                case InverterParameterId.ShutterHighVelocityDuration:
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.BrakeActivatePercent:
                case InverterParameterId.BrakeReleaseTime:
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.CurrentError:
                    var errorMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes(inverter.IsFault ? (ushort)random.Next(1, 100) : (ushort)0));
                    // simulate all inverters in fault
                    //var errorMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)random.Next(1, 100)));
                    result = client.Client.Send(errorMessage);
                    break;

                case InverterParameterId.BlockDefinition:
                    inverter.BlockDefinitions = (List<InverterBlockDefinition>)message.Payload;
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.BlockWrite:
                    var blockRead = message.ConvertPayloadToBlockRead(inverter.BlockDefinitions);
                    for (var iblock = 0; iblock < inverter.BlockDefinitions.Count; iblock++)
                    {
                        switch (inverter.BlockDefinitions[iblock].ParameterId)
                        {
                            case InverterParameterId.TableTravelTargetSpeeds:
                                inverter.TargetSpeed[inverter.CurrentAxis] = Math.Max((int)blockRead[iblock], inverter.TargetSpeed[inverter.CurrentAxis]);
                                break;

                            case InverterParameterId.PositionTargetPosition:
                                inverter.TargetPosition[inverter.CurrentAxis] = inverter.Impulses2millimeters((int)blockRead[iblock]);
                                inverter.StartPosition[inverter.CurrentAxis] = inverter.AxisPosition;
                                break;

                            case InverterParameterId.PositionTargetSpeed:
                                inverter.TargetSpeed[inverter.CurrentAxis] = (int)blockRead[iblock];
                                break;

                            case InverterParameterId.PositionAcceleration:
                                inverter.TargetAcceleration[inverter.CurrentAxis] = (int)blockRead[iblock];
                                break;

                            case InverterParameterId.PositionDeceleration:
                                inverter.TargetAcceleration[inverter.CurrentAxis] = (int)blockRead[iblock];
                                break;

                            case InverterParameterId.TableTravelTableIndex:
                                inverter.TableIndex = (int)blockRead[iblock];
                                break;

                            case InverterParameterId.TableTravelTargetPosition:
                                if (inverter.TableIndex < (int)InverterTableIndex.TableTravelP1)
                                {
                                    inverter.TargetPosition[inverter.CurrentAxis] = inverter.Impulses2millimeters((int)blockRead[iblock]);
                                    inverter.StartPosition[inverter.CurrentAxis] = inverter.AxisPosition;
                                    inverter.IsStartedOnBoard = this.remoteIOs[0].Inputs[(int)IoPorts.DrawerInMachineSide].Value
                                        && this.remoteIOs[0].Inputs[(int)IoPorts.DrawerInOperatorSide].Value;
                                }
                                else
                                {
                                    var switchId = inverter.TableIndex - (int)InverterTableIndex.TableTravelP1;
                                    inverter.SwitchPositions[inverter.CurrentAxis][switchId] = inverter.Impulses2millimeters((int)blockRead[iblock]);
                                }
                                break;
                        }
                    }
                    result = client.Client.Send(message.ToBytes());
                    break;

                case InverterParameterId.BlockRead:
                    var blockValues = new object[inverter.BlockDefinitions.Count];
                    for (var iblock = 0; iblock < inverter.BlockDefinitions.Count; iblock++)
                    {
                        switch (inverter.BlockDefinitions[iblock].ParameterId)
                        {
                            case InverterParameterId.TableTravelTableIndex:
                                blockValues[iblock] = (short)inverter.TableIndex;
                                break;

                            case InverterParameterId.TableTravelTargetSpeeds:
                                blockValues[iblock] = inverter.TargetSpeed[inverter.CurrentAxis];
                                break;

                            case InverterParameterId.TableTravelTargetAccelerations:
                                blockValues[iblock] = inverter.TargetAcceleration[inverter.CurrentAxis];    // this value is not correct, just to test the message
                                break;

                            case InverterParameterId.TableTravelTargetDecelerations:
                                blockValues[iblock] = inverter.TargetDeceleration[inverter.CurrentAxis];    // this value is not correct, just to test the message
                                break;
                        }
                    }
                    {
                        var replyMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, Encoding.ASCII.GetBytes(InverterMessage.FormatBlockWrite(blockValues)));
                        result = client.Client.Send(replyMessage);
                    }
                    break;

                case InverterParameterId.AxisChanged:
                    var axisChangedMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)inverter.AxisChanged));
                    result = client.Client.Send(axisChangedMessage);
                    inverter.AxisChanged = random.Next(0, 3);
                    break;

                case InverterParameterId.ActiveDataset:
                    result = client.Client.Send(message.ToBytes());
                    break;

                default:
                    // simulate inverter programming
                    var rxMessage = this.FormatMessage(message.ToBytes(), (InverterRole)message.SystemIndex, message.DataSetIndex, BitConverter.GetBytes((ushort)random.Next(0, 100)));
                    result = client.Client.Send(rxMessage);

                    // show unexpected messages
                    // Debug.Assert(true);
                    break;
            }
        }

        private void UpdateInverter(InverterModel inverter)
        {
            if ((inverter.ControlWord & 0x0100) > 0)       // Halt
            {
                inverter.IsFault = true;
            }
            else if ((inverter.ControlWord & 0x0080) > 0)   // Reset Fault
            {
                inverter.IsFault = false;
            }

            if (!inverter.IsFault)
            {
                // Switch On
                inverter.IsReadyToSwitchOn = inverter.IsVoltageEnabled;
                inverter.IsSwitchedOn = (inverter.ControlWord & 0x0001) > 0 && inverter.IsReadyToSwitchOn;

                // Enable Voltage
                inverter.IsVoltageEnabled = (inverter.ControlWord & 0x0002) > 0;
            }

            // Quick Stop
            inverter.IsQuickStopTrue = (inverter.ControlWord & 0x0004) > 0;
            if (!inverter.IsQuickStopTrue)
            {
                inverter.IsOperationEnabled = false;
            }
            else
            {
                inverter.IsOperationEnabled = (inverter.ControlWord & 0x0008) > 0;
            }

            if (!inverter.IsOperationEnabled)
            {
                inverter.IsTargetReached = false;
                inverter.StatusWord &= 0xEFFF;  // Reset Set-Point Acknowledge
            }

            inverter.CurrentAxis = (!inverter.IsHorizontalAxis) ? Axis.Vertical : Axis.Horizontal;
        }

        private void UpdateRemoteIO(IODeviceModel device)
        {
            // If any security signal is received, reset run status
            if (!this.RemoteIOs01.Outputs[(int)IoPorts.PowerEnable].Value ||
                !device.Inputs[(int)IoPorts.MushroomEmergency].Value ||
                !device.Inputs[(int)IoPorts.MicroCarterLeftSideBay].Value ||
                !device.Inputs[(int)IoPorts.MicroCarterRightSideBay].Value ||
                !device.Inputs[(int)IoPorts.AntiIntrusionBarrierBay].Value
                )
            {
                // Reset run status
                this.remoteIOs.ToList().ForEach(x => x.Inputs[(int)IoPorts.NormalState].Value = false);
            }
            // If reset security impulse is received and there are no emergency button pushed, set run status
            else if (this.RemoteIOs01.Outputs[(int)IoPorts.ResetSecurity].Value && this.remoteIOs.All(x => x.Inputs[(int)IoPorts.MushroomEmergency].Value))
            {
                if (!this.Inverters.Any(x => x.IsFault))
                {
                    // Set run status
                    this.remoteIOs
                         .Single(io => io.Id == 0)
                         .Inputs[(int)IoPorts.NormalState]
                         .Value = true;

                    // Power up inverters
                    this.Inverters.ToList().ForEach(x => x.DigitalIO[(int)InverterSensors.ANG_HardwareSensorSTO].Value = true);
                }
            }

            // Set inverter cumulative fault if any inverter is in fault
            this.remoteIOs[0].Inputs[(int)IoPorts.InverterInFault].Value = this.Inverters.Any(x => x.IsFault);

            // If run status is off, trigger emergency stop on inverters
            if (!this.RemoteIOs01.Inputs[(int)IoPorts.NormalState].Value)
            {
                this.Inverters.ToList().ForEach(x => x.EmergencyStop());
            }
            this.RemoteIOs01.Inputs[(int)IoPorts.ElevatorMotorFeedback].Value = this.RemoteIOs01.Outputs[(int)IoPorts.ElevatorMotor].Value;
            this.RemoteIOs01.Inputs[(int)IoPorts.CradleMotorFeedback].Value = this.RemoteIOs01.Outputs[(int)IoPorts.CradleMotor].Value;
        }

        #endregion
    }
}
