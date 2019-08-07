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
using Ferretto.VW.Simulator.Services.Interfaces;
using Ferretto.VW.Simulator.Services.Models;
using static Ferretto.VW.Simulator.Services.BufferUtility;
using Microsoft.Extensions.Logging;
using NLog;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator.Services
{
    internal class MachineService : BindableBase, IMachineService
    {
        #region Fields

        public byte[] Buffer;

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
            this.Inverters.Add(new InverterModel() { Id = 0, InverterType = InverterType.Ang });
            this.Inverters.Add(new InverterModel() { Id = 1, InverterType = InverterType.Ang, Enabled = false });
            this.Inverters.Add(new InverterModel() { Id = 2, InverterType = InverterType.Agl });
            this.Inverters.Add(new InverterModel() { Id = 3, InverterType = InverterType.Acu });
            this.Inverters.Add(new InverterModel() { Id = 4, InverterType = InverterType.Agl });
            this.Inverters.Add(new InverterModel() { Id = 5, InverterType = InverterType.Acu, Enabled = false });
            this.Inverters.Add(new InverterModel() { Id = 6, InverterType = InverterType.Acu, Enabled = false }); //da sistemare
            this.Inverters.Add(new InverterModel() { Id = 7, InverterType = InverterType.Acu, Enabled = false }); //da sistemare

            this.remoteIOs.Add(new IODeviceModel() { Id = 0 });
            this.remoteIOs.Add(new IODeviceModel() { Id = 1 });
            this.remoteIOs.Add(new IODeviceModel() { Id = 2, Enabled = false });
        }

        #endregion

        #region Properties

        public ObservableCollection<InverterModel> Inverters { get; set; }

        public InverterModel Inverters00 { get => this.Inverters[0]; set { var inv = this.Inverters[0]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters01 { get => this.Inverters[1]; set { var inv = this.Inverters[0]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters02 { get => this.Inverters[2]; set { var inv = this.Inverters[0]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters03 { get => this.Inverters[3]; set { var inv = this.Inverters[0]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters04 { get => this.Inverters[4]; set { var inv = this.Inverters[0]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters05 { get => this.Inverters[5]; set { var inv = this.Inverters[0]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters06 { get => this.Inverters[6]; set { var inv = this.Inverters[0]; this.SetProperty(ref inv, value); } }

        public InverterModel Inverters07 { get => this.Inverters[7]; set { var inv = this.Inverters[0]; this.SetProperty(ref inv, value); } }

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

            Task.Run(() => this.AcceptClient(this.listenerInverter, this.cts.Token, (client, message) => this.ReplyInverter(client, message)));
            if (this.RemoteIOs01.Enabled)
            {
                Task.Run(() => this.AcceptClient(this.listenerIoDriver1, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message, 0)));
            }

            if (this.RemoteIOs02.Enabled)
            {
                Task.Run(() => this.AcceptClient(this.listenerIoDriver2, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message, 1)));
            }

            if (this.RemoteIOs03.Enabled)
            {
                Task.Run(() => this.AcceptClient(this.listenerIoDriver3, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message, 2)));
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
                            if (socket.Poll(5000, SelectMode.SelectRead))
                            {
                                var bytes = socket.Receive(buffer);
                                if (bytes > 0)
                                {
                                    byte[] message = new byte[bytes];
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
                    if (extractedMessage.Length >= headerLenght + payloadLength)
                    {
                        payload = new byte[payloadLength];
                        Array.Copy(extractedMessage, headerLenght, payload, 0, payloadLength);

                        if (payload.Length == 2)
                        {
                            ushortPayload = BitConverter.ToUInt16(payload, 0);
                        }
                    }

                    int result = 0;
                    switch ((InverterParameterId)parameterId)
                    {
                        case InverterParameterId.ControlWordParam:
                            inverter.ControlWord = ushortPayload;
                            //this.UpdateInverter(inverter);
                            result = client.Client.Send(extractedMessage);
                            break;

                        case InverterParameterId.DigitalInputsOutputs:
                            var values = this.Inverters.GroupBy(x => x.Id / 2).Select(x => x.First().GetDigitalIO() + (x.Last().GetDigitalIO() << 8)).ToArray();
                            string inputValues = $" {string.Join(" ", values)} ";
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
                        case InverterParameterId.PositionAccelerationParam:
                        case InverterParameterId.PositionDecelerationParam:
                        case InverterParameterId.PositionTargetPositionParam:
                        case InverterParameterId.PositionTargetSpeedParam:
                        case InverterParameterId.ShutterTargetVelocityParam:
                            result = client.Client.Send(extractedMessage);
                            break;

                        case InverterParameterId.StatusWordParam:
                            switch (inverter.OperationMode)
                            {
                                case InverterOperationMode.Homing:
                                    inverter.BuildHomingStatusWord();
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
                            var actualPositionMessage = this.FormatMessage(extractedMessage, systemIndex, dataSetIndex, BitConverter.GetBytes(++inverter.AxisPosition));
                            result = client.Client.Send(actualPositionMessage);
                            break;

                        case InverterParameterId.StatusDigitalSignals:
                        case InverterParameterId.ShutterTargetPosition:
                            break;

                        default:
                            if (System.Diagnostics.Debugger.IsAttached)
                            {
                                System.Diagnostics.Debugger.Break();
                            }
                            break;
                    }
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
                //if (extractedMessages.Count > 1 && Debugger.IsAttached)
                //{
                //    Debugger.Break();
                //}

                foreach (var extractedMessage in extractedMessages)
                {
                    var length = extractedMessage[0];
                    var firmwareProtocol = extractedMessage[1];
                    var codeOperation = extractedMessage[2];
                    var outputs = (from x in Enumerable.Range(0, 8)
                                   let binary = Convert.ToString(device.FirmwareVersion == 0x10 ? extractedMessage[3] : extractedMessage[4], 2).PadLeft(8, '0')
                                   select new { Value = binary[x] == '1' ? true : false, Description = (7 - x).ToString() }).Reverse().ToArray();
                    device.Outputs = outputs.Select(x => new BitModel(x.Description, x.Value)).ToList();

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
                            byte[] inputs = BitConverter.GetBytes(device.InputsValue);
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
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void UpdateInverter(InverterModel inverter)
        {
            if ((inverter.ControlWord & 0x0080) > 0)            // Reset fault
            {
                inverter.IsFault = false;
            }
            else if ((inverter.ControlWord & 0x0001) > 0)       // SwitchOn
            {
                inverter.IsSwitchedOn = true;
            }
        }

        private void UpdateRemoteIO(IODeviceModel device)
        {
            // Logic
            if (!device.Outputs[(int)IoPorts.PowerEnable].Value)
            {
                // Set run status
                device.Inputs[(int)IoPorts.NormalState].Value = false;
            }
            else if (device.Outputs[(int)IoPorts.ResetSecurity].Value)
            {
                // Set run status
                device.Inputs[(int)IoPorts.NormalState].Value = true;
            }
        }

        #endregion
    }
}
