using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
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

        private const int IMPULSES_ENCODER_PER_ROUND = 77;

        private readonly TcpListener listenerInverter = new TcpListener(IPAddress.Any, 17221);

        private readonly TcpListener listenerIoDriver1 = new TcpListener(IPAddress.Any, 19550);

        private readonly TcpListener listenerIoDriver2 = new TcpListener(IPAddress.Any, 19551);

        private readonly TcpListener listenerIoDriver3 = new TcpListener(IPAddress.Any, 19552);

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ObservableCollection<IODeviceModel> remoteIOs = new ObservableCollection<IODeviceModel>();

        private CancellationTokenSource cts = new CancellationTokenSource();

        #endregion

        #region Constructors

        public MachineService()
        {
            this.Inverters = new ObservableCollection<InverterModel>();
            this.Inverters.Add(new InverterModel(InverterType.Ang) { Id = 0 });
            this.Inverters.Add(new InverterModel(InverterType.Ang) { Id = 1, Enabled = false });
            this.Inverters.Add(new InverterModel(InverterType.Agl) { Id = 2 });
            this.Inverters.Add(new InverterModel(InverterType.Acu) { Id = 3 });
            this.Inverters.Add(new InverterModel(InverterType.Agl) { Id = 4 });
            this.Inverters.Add(new InverterModel(InverterType.Acu) { Id = 5, Enabled = false });
            this.Inverters.Add(new InverterModel(InverterType.Agl) { Id = 6, Enabled = false }); //da sistemare
            this.Inverters.Add(new InverterModel(InverterType.Acu) { Id = 7, Enabled = false }); //da sistemare

            this.remoteIOs.Add(new IODeviceModel() { Id = 0 });
            this.remoteIOs.Add(new IODeviceModel() { Id = 1 });
            this.remoteIOs.Add(new IODeviceModel() { Id = 2, Enabled = false });
        }

        #endregion

        #region Properties

        public ObservableCollection<InverterModel> Inverters { get; set; }

        public InverterModel Inverters00 { get => this.Inverters[0]; set { var inv = this.Inverters[0]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters01 { get => this.Inverters[1]; set { var inv = this.Inverters[1]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters02 { get => this.Inverters[2]; set { var inv = this.Inverters[2]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters03 { get => this.Inverters[3]; set { var inv = this.Inverters[3]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters04 { get => this.Inverters[4]; set { var inv = this.Inverters[4]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters05 { get => this.Inverters[5]; set { var inv = this.Inverters[5]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters06 { get => this.Inverters[6]; set { var inv = this.Inverters[6]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters07 { get => this.Inverters[7]; set { var inv = this.Inverters[7]; this.SetProperty(ref inv, value); } }

        public bool IsStartedSimulator { get; private set; }

        public IODeviceModel RemoteIOs01 { get => this.remoteIOs[0]; set { var ios = this.remoteIOs[0]; this.SetProperty(ref ios, value); } }

        public IODeviceModel RemoteIOs02 { get => this.remoteIOs[1]; set { var ios = this.remoteIOs[1]; this.SetProperty(ref ios, value); } }

        public IODeviceModel RemoteIOs03 { get => this.remoteIOs[2]; set { var ios = this.remoteIOs[2]; this.SetProperty(ref ios, value); } }

        #endregion

        #region Methods

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

        private byte[] FormatMessage(byte[] message, InverterRole systemIndex, byte dataSetIndex, byte[] inputValues)
        {
            int byteLength;
            byte[] byteMessage;
            byteLength = 0x04 + inputValues.Length;
            byteMessage = new byte[byteLength + 2];
            byteMessage[0] = 0x00;
            byteMessage[1] = (byte)(byteLength);
            byteMessage[2] = (byte)systemIndex;
            byteMessage[3] = dataSetIndex;
            byteMessage[4] = message[4];
            byteMessage[5] = message[5];
            Array.Copy(inputValues, 0, byteMessage, 6, inputValues.Length);
            return byteMessage;
        }

        private int Impulses2millimeters(int value)
        {
            return value / IMPULSES_ENCODER_PER_ROUND;
        }

        private void ManageClient(TcpClient client, CancellationToken token, Action<TcpClient, byte[]> messageHandler)
        {
            using (client)
            {
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

        private int Millimeters2Impulses(int value)
        {
            return value * IMPULSES_ENCODER_PER_ROUND;
        }

        private void ReplyInverter(TcpClient client, byte[] message)
        {
            const int headerLenght = 6;
            this.Buffer = this.Buffer.AppendArrays(message, message.Length);
            if (this.Buffer.Length >= headerLenght && this.Buffer.Length >= this.Buffer[1] + 2)
            {
                var extractedMessages = GetMessagesWithHeaderLengthToEnqueue(ref this.Buffer, 4, 1, 2);

                foreach (var extractedMessage in extractedMessages)
                {
                    var isWriteMessage = (extractedMessage[0] & 0x80) > 0;
                    var isError = (extractedMessage[0] & 0x40) > 0;
                    var payloadLength = extractedMessage[1] - 4;
                    var systemIndex = (InverterRole)extractedMessage[2];
                    var dataSetIndex = extractedMessage[3];
                    var parameterId = BitConverter.ToInt16(extractedMessage, 4);

                    var inverter = this.Inverters.First(x => x.InverterRole == systemIndex);

                    byte[] payload = null;
                    ushort ushortPayload = 0;
                    uint uintPayload = 0;
                    if (extractedMessage.Length >= headerLenght + payloadLength)
                    {
                        payload = new byte[payloadLength];
                        Array.Copy(extractedMessage, headerLenght, payload, 0, payloadLength);

                        if (payload.Length == 2)
                        {
                            ushortPayload = BitConverter.ToUInt16(payload, 0);
                        }
                        else if (payload.Length == 4)
                        {
                            uintPayload = BitConverter.ToUInt32(payload, 0);
                        }
                    }

                    var result = 0;
                    switch ((InverterParameterId)parameterId)
                    {
                        case InverterParameterId.ControlWordParam:
                            inverter.ControlWord = ushortPayload;
                            inverter.RefreshControlWordArray();
                            this.UpdateInverter(inverter);
                            result = client.Client.Send(extractedMessage);
                            break;

                        case InverterParameterId.DigitalInputsOutputs:
                            var values = this.Inverters.GroupBy(x => x.Id / 2).Select(x => x.First().GetDigitalIO() + (x.Last().GetDigitalIO() << 8)).ToArray();
                            var inputValues = $" {string.Join(" ", values)} ";
                            var ioStatusMessage = this.FormatMessage(extractedMessage, systemIndex, dataSetIndex, Encoding.ASCII.GetBytes(inputValues));
                            result = client.Client.Send(ioStatusMessage);
                            break;

                        case InverterParameterId.SetOperatingModeParam:
                            inverter.OperationMode = (InverterOperationMode)ushortPayload;
                            result = client.Client.Send(extractedMessage);
                            break;

                        case InverterParameterId.HomingCreepSpeedParam:
                        case InverterParameterId.HomingFastSpeedParam:
                        case InverterParameterId.HomingAcceleration:
                            result = client.Client.Send(extractedMessage);
                            break;

                        case InverterParameterId.PositionAccelerationParam:
                            inverter.TargetAcceleration[inverter.CurrentAxis] = (int)uintPayload;
                            result = client.Client.Send(extractedMessage);
                            break;

                        case InverterParameterId.PositionDecelerationParam:
                            inverter.TargetDeceleration[inverter.CurrentAxis] = (int)uintPayload;
                            result = client.Client.Send(extractedMessage);
                            break;

                        case InverterParameterId.PositionTargetPositionParam:
                            inverter.TargetPosition[inverter.CurrentAxis] = this.Impulses2millimeters((int)uintPayload);
                            inverter.StartPosition[inverter.CurrentAxis] = inverter.AxisPosition;
                            result = client.Client.Send(extractedMessage);
                            break;

                        case InverterParameterId.PositionTargetSpeedParam:
                            inverter.TargetSpeed[inverter.CurrentAxis] = (int)uintPayload;
                            result = client.Client.Send(extractedMessage);
                            break;

                        case InverterParameterId.ShutterTargetVelocityParam:
                            inverter.SpeedRate = (int)uintPayload;
                            result = client.Client.Send(extractedMessage);
                            break;

                        case InverterParameterId.StatusWordParam:
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

                                default:
                                    if (System.Diagnostics.Debugger.IsAttached)
                                    {
                                        System.Diagnostics.Debugger.Break();
                                    }
                                    break;
                            }
                            var statusWordMessage = this.FormatMessage(extractedMessage, systemIndex, dataSetIndex, BitConverter.GetBytes((ushort)inverter.StatusWord));
                            result = client.Client.Send(statusWordMessage);
                            break;

                        case InverterParameterId.ActualPositionShaft:
                            var impulses = this.Millimeters2Impulses(inverter.AxisPosition);
                            var actualPositionMessage = this.FormatMessage(extractedMessage, systemIndex, dataSetIndex, BitConverter.GetBytes(impulses));
                            result = client.Client.Send(actualPositionMessage);
                            break;

                        case InverterParameterId.StatusDigitalSignals:
                            result = client.Client.Send(extractedMessage);
                            break;

                        case InverterParameterId.ShutterTargetPosition:
                            inverter.TargetShutterPosition = (int)ushortPayload;
                            result = client.Client.Send(extractedMessage);
                            break;

                        default:
                            if (System.Diagnostics.Debugger.IsAttached)
                            {
                                System.Diagnostics.Debugger.Break();
                            }
                            break;
                    }

                    Thread.Sleep(DELAY_INVERTER_CLIENT);
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

                    var result = client.Client.Send(responseMessage);
                    Thread.Sleep(DELAY_IO_CLIENT);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void UpdateInverter(InverterModel inverter)
        {
            if ((inverter.ControlWord & 0x0100) > 0)       // Halt
            {
                inverter.IsFault = true;
            }
            else if ((inverter.ControlWord & 0x0080) > 0)   // Reset fault
            {
                inverter.IsFault = false;
                this.remoteIOs[0].Inputs[(int)IoPorts.InverterInFault].Value = false;
            }

            // Switch On
            inverter.IsReadyToSwitchOn = inverter.IsVoltageEnabled;
            inverter.IsSwitchedOn = (inverter.ControlWord & 0x0001) > 0 && inverter.IsReadyToSwitchOn;

            // Enable Voltage
            inverter.IsVoltageEnabled = (inverter.ControlWord & 0x0002) > 0;

            // Quick Stop
            inverter.IsQuickStopTrue = (inverter.ControlWord & 0x0004) > 0;
            if (!inverter.IsQuickStopTrue)                      // Quick stop
            {
                inverter.IsOperationEnabled = false;
            }
            else
            {
                inverter.IsOperationEnabled = (inverter.ControlWord & 0x0008) > 0;   // Enable Operation
            }

            inverter.CurrentAxis = (inverter.IsHorizontalAxis) ? Axis.Horizontal : Axis.Vertical;
        }

        private void UpdateRemoteIO(IODeviceModel device)
        {
            // Logic
            if (!this.RemoteIOs01.Outputs[(int)IoPorts.PowerEnable].Value || !device.Inputs[(int)IoPorts.MushroomEmergency].Value)
            {
                // Reset run status
                this.remoteIOs.ToList().ForEach(x => x.Inputs[(int)IoPorts.NormalState].Value = false);
            }
            else if (this.RemoteIOs01.Outputs[(int)IoPorts.ResetSecurity].Value && this.remoteIOs.All(x => x.Inputs[(int)IoPorts.MushroomEmergency].Value))
            {
                // Set run status
                this.remoteIOs.ToList().ForEach(x => x.Inputs[(int)IoPorts.NormalState].Value = true);

                // Power up inverters
                this.Inverters.ToList().ForEach(x => x.DigitalIO[(int)InverterSensors.ANG_HardwareSensorSTO].Value = true);
            }
            foreach (var inverter in this.Inverters)
            {
                if (inverter.IsFault)
                {
                    this.remoteIOs[0].Inputs[(int)IoPorts.InverterInFault].Value = true;
                    break;
                }
            }
        }

        #endregion
    }
}
