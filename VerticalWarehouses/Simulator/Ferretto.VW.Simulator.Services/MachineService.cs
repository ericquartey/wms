using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Simulator.Services.Interfaces;
using Ferretto.VW.Simulator.Services.Models;
using Microsoft.Extensions.Logging;
using NLog;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator.Services
{
    internal class MachineService : BindableBase, IMachineService
    {
        #region Fields

        private readonly TcpListener listenerInverter = new TcpListener(IPAddress.Any, 17221);

        private readonly TcpListener listenerIoDriver1 = new TcpListener(IPAddress.Any, 19550);

        private readonly TcpListener listenerIoDriver2 = new TcpListener(IPAddress.Any, 19551);

        private readonly TcpListener listenerIoDriver3 = new TcpListener(IPAddress.Any, 19552);

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private CancellationTokenSource cts = new CancellationTokenSource();

        #endregion

        #region Constructors

        public MachineService()
        {
            this.Inverters = new List<InverterModel>();
            this.Inverters.Add(new InverterModel() { Id = 0, InverterType = InverterType.Ang });
            //this.Inverters.Add(new InverterModel() { Id = 1, InverterType = InverterType.Ang });
            this.Inverters.Add(new InverterModel() { Id = 2, InverterType = InverterType.Agl });
            this.Inverters.Add(new InverterModel() { Id = 3, InverterType = InverterType.Acu });
            this.Inverters.Add(new InverterModel() { Id = 4, InverterType = InverterType.Agl });
            //this.Inverters.Add(new InverterModel() { Id = 5, InverterType = InverterType.Acu });

            this.RemoteIOs = new List<IODeviceModel>();
            this.RemoteIOs.Add(new IODeviceModel() { Id = 0 });
            this.RemoteIOs.Add(new IODeviceModel() { Id = 1 });
            this.RemoteIOs.Add(new IODeviceModel() { Id = 2 });

            this.RemoteIOs[0].IOs[0] = true;
        }

        #endregion

        #region Properties

        public bool ForceSicurity { get; private set; }

        public List<InverterModel> Inverters { get; set; }

        public bool IsStartedSimulator { get; private set; }

        public List<IODeviceModel> RemoteIOs { get; set; }

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
            this.listenerIoDriver1.Start();
            this.listenerIoDriver2.Start();
            //this.listenerIoDriver3.Start();

            Task.Run(() => this.AcceptClient(this.listenerInverter, this.cts.Token, (client, message) => this.ReplyInverter(client, message)));
            Task.Run(() => this.AcceptClient(this.listenerIoDriver1, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message, 0)));
            Task.Run(() => this.AcceptClient(this.listenerIoDriver2, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message, 1)));
            //Task.Run(() => this.AcceptClient(this.listenerIoDriver3, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message, 2)));

            await Task.Delay(100);
            this.IsStartedSimulator = true;

            this.RaisePropertyChanged(nameof(this.IsStartedSimulator));
        }

        public async Task ProcessStopSimulatorAsync()
        {
            this.Logger.Trace("1:ProcessStopSimulator");

            this.cts.Cancel();
            this.listenerInverter.Stop();
            this.listenerIoDriver1.Stop();
            this.listenerIoDriver2.Stop();
            //this.listenerIoDriver3.Stop();

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
                            if (socket.Poll(0, SelectMode.SelectRead))
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
                        Task.Delay(50);
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
            if (message.Length >= headerLenght)
            {
                var isWriteMessage = (message[0] & 0x80) > 0;
                var isError = (message[0] & 0x40) > 0;
                var payloadLength = message[1] - 4;
                var systemIndex = (InverterRole)message[2];
                var dataSetIndex = message[3];
                var parameterId = BitConverter.ToInt16(message, 4);

                var inverter = this.Inverters.First(x => x.InverterRole == systemIndex);

                byte[] payload = null;
                ushort ushortPayload = 0;
                if (message.Length >= headerLenght + payloadLength)
                {
                    payload = new byte[payloadLength];
                    Array.Copy(message, headerLenght, payload, 0, payloadLength);

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
                        result = client.Client.Send(message);
                        break;

                    case InverterParameterId.DigitalInputsOutputs:
                        var values = this.Inverters.GroupBy(x => x.Id / 2).Select(x => x.First().GetDigitalIO() + x.Last().GetDigitalIO() >> 8).ToArray();
                        string inputValues = $" {string.Join(" ", values)} ";
                        var ioStatusMessage = this.FormatMessage(message, systemIndex, dataSetIndex, Encoding.ASCII.GetBytes(inputValues));
                        result = client.Client.Send(ioStatusMessage);
                        break;

                    case InverterParameterId.SetOperatingModeParam:
                        inverter.OperationMode = (InverterOperationMode)ushortPayload;
                        result = client.Client.Send(message);
                        break;

                    case InverterParameterId.HomingCreepSpeedParam:
                    case InverterParameterId.HomingFastSpeedParam:
                    case InverterParameterId.HomingAcceleration:
                    case InverterParameterId.PositionAccelerationParam:
                    case InverterParameterId.PositionDecelerationParam:
                    case InverterParameterId.PositionTargetPositionParam:
                    case InverterParameterId.PositionTargetSpeedParam:
                    case InverterParameterId.ShutterTargetVelocityParam:
                        result = client.Client.Send(message);
                        break;

                    case InverterParameterId.StatusWordParam:
                        //inverter.StatusWord = ushortPayload;
                        switch(inverter.OperationMode)
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
                        var statusWordMessage = this.FormatMessage(message, systemIndex, dataSetIndex, BitConverter.GetBytes((ushort)inverter.StatusWord));
                        result = client.Client.Send(statusWordMessage);
                        break;

                    case InverterParameterId.ActualPositionShaft:
                        var actualPositionMessage = this.FormatMessage(message, systemIndex, dataSetIndex, BitConverter.GetBytes(++inverter.AxisPosition));
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
            else
            {
                throw new NotSupportedException();
            }
        }

        private byte [] FormatMessage(byte[] message, InverterRole systemIndex, byte dataSetIndex, byte [] inputValues)
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


        private void ReplyIoDriver(TcpClient client, byte[] message, int index)
        {
            const int headerLenght = 4;
            const int NBYTES_RECEIVE = 15;
            const int NBYTES_RECEIVE_CFG = 3;

            var device = this.RemoteIOs.First(x => x.Id == index);

            if (message.Length >= headerLenght)
            {
                var length = message[0];
                var relProtocol = message[1];
                var codeOperation = message[2];

                byte[] responseMessage = null;
                switch (codeOperation)
                {
                    case 0x00: // Data
                        responseMessage = new byte[NBYTES_RECEIVE];
                        responseMessage[0] = NBYTES_RECEIVE;            // nBytes
                        responseMessage[1] = device.FirmwareVersion;    // fwRelease
                        responseMessage[2] = 0x00;                      // Code op   0x00: data, 0x06: configuration
                        responseMessage[3] = 0x00;                      // error code
                        Array.Copy(message, 3, responseMessage, 4, 1);  // output values echo
                        byte[] inputs = BitConverter.GetBytes(device.IOValue);
                        responseMessage[5] = inputs[0];
                        responseMessage[6] = inputs[1];
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
                var result = client.Client.Send(responseMessage);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }
}
