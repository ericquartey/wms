using System;
using System.Collections.Generic;
using System.Net;
using System.Net.EnIPStack;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface;

namespace Ferretto.VW.MAS.InverterDriver
{
    public class SocketTransportMock : ISocketTransportMock, IDisposable
    {
        #region Fields

        private readonly Timer homingTimer;

        private readonly ManualResetEventSlim readCompleteEventSlim;

        private readonly Timer targetTimer;

        private int axisPosition;

        private ushort controlWord;

        private bool disposed;

        private int homingTickCount;

        private bool homingTimerActive;

        private InverterMessage lastWriteMessage;

        private InverterOperationMode operatingMode;

        private ushort statusWord;

        private int targetTickCount;

        private bool targetTimerActive;

        #endregion

        #region Constructors

        public SocketTransportMock()
        {
            this.operatingMode = InverterOperationMode.Velocity;

            this.readCompleteEventSlim = new ManualResetEventSlim(false);

            this.lastWriteMessage = new InverterMessage((short)0x00, InverterParameterId.ControlWord);

            this.homingTimer = new Timer(this.HomingTick, null, -1, Timeout.Infinite);

            this.homingTimerActive = false;
            this.targetTimer = new Timer(this.TargetTick, null, -1, Timeout.Infinite);

            this.targetTimerActive = false;

            this.axisPosition = 0;
        }

        #endregion

        #region Events

        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        public event EventHandler<ImplicitReceivedEventArgs> ImplicitReceivedChanged;

        #endregion

        #region Properties

        public bool IsConnected => true;

        public bool IsConnectedUdp { get; set; }

        #endregion

        #region Methods

        public void Configure(IPAddress inverterAddress, int sendPort, IEnumerable<int> nodeList = null)
        {
        }

        public Task ConnectAsync()
        {
            return Task.CompletedTask;
        }

        public void Disconnect()
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool ExplicitMessage(ushort classId, uint instanceId, ushort attributeId, CIPServiceCodes serviceId, byte[] data, out byte[] receive, out int length)
        {
            throw new NotImplementedException();
        }

        public bool ImplicitMessageWrite(byte[] data, int node)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            if (this.disposed)
            {
                throw new InvalidOperationException($"Cannot access the disposed instance of {this.GetType().Name}.");
            }

            await Task.Delay(5, stoppingToken);

            if (this.readCompleteEventSlim.Wait(Timeout.Infinite, stoppingToken))
            {
                this.readCompleteEventSlim.Reset();

                InverterMessage currentMessage;
                lock (this.lastWriteMessage)
                {
                    currentMessage = this.lastWriteMessage;
                }

                if (currentMessage.IsWriteMessage)
                {
                    return currentMessage.ToBytes();
                }

                if (currentMessage.IsReadMessage)
                {
                    return this.BuildReadMessage(currentMessage);
                }

                return this.BuildRawStatusMessage();
            }

            return null;
        }

        public bool SDOMessage(byte nodeId, ushort index, byte subindex, bool isWriteMessage, byte[] data, out byte[] receive, out int length)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, CancellationToken stoppingToken)
        {
            return await this.ParseWriteMessage(inverterMessage, stoppingToken);
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, int delay, CancellationToken stoppingToken)
        {
            await Task.Delay(delay, stoppingToken);
            return await this.WriteAsync(inverterMessage, stoppingToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.readCompleteEventSlim?.Dispose();
                    this.homingTimer?.Dispose();
                    this.targetTimer.Dispose();
                }

                this.disposed = true;
            }
        }

        private byte[] BuildDigitalInputsMessage(InverterMessage currentMessage)
        {
            byte systemIndex;
            InverterParameterId parameterId;
            byte dataSet;

            lock (this.lastWriteMessage)
            {
                systemIndex = (byte)currentMessage.SystemIndex;
                parameterId = currentMessage.ParameterId;
                dataSet = currentMessage.DataSetIndex;
            }

            var inputValues = " 8 3 1 1 ";

            var rawMessage = new byte[6 + inputValues.Length];

            rawMessage[0] = 0x00;
            rawMessage[1] = (byte)(0x04 + inputValues.Length);
            rawMessage[2] = systemIndex;
            rawMessage[3] = dataSet;

            var parameterBytes = BitConverter.GetBytes((ushort)parameterId);

            rawMessage[4] = parameterBytes[0];
            rawMessage[5] = parameterBytes[1];

            var payloadBytes = Encoding.ASCII.GetBytes(inputValues);
            Array.Copy(payloadBytes, 0, rawMessage, 6, payloadBytes.Length);

            return rawMessage;
        }

        private void BuildHomingStatusWord()
        {
            //SwitchON
            if ((this.controlWord & 0x0001) > 0)
            {
                this.statusWord |= 0x0002;
            }
            else
            {
                this.statusWord &= 0xFFFD;
            }

            //EnableVoltage
            if ((this.controlWord & 0x0002) > 0)
            {
                this.statusWord |= 0x0001;
                this.statusWord |= 0x0010;
            }
            else
            {
                this.statusWord &= 0xFFFE;
                this.statusWord &= 0xFFEF;
            }

            //QuickStop
            if ((this.controlWord & 0x0004) > 0)
            {
                this.statusWord |= 0x0020;
            }
            else
            {
                this.statusWord &= 0xFFDF;
            }

            //EnableOperation
            if ((this.controlWord & 0x0008) > 0)
            {
                this.statusWord |= 0x0004;
            }
            else
            {
                this.statusWord &= 0xFFFB;
            }

            //StartHoming
            if ((this.controlWord & 0x0010) > 0)
            {
                if (!this.homingTimerActive)
                {
                    this.homingTimer.Change(0, 1000);
                    this.homingTimerActive = true;
                    this.axisPosition = 0;
                }
            }
            else
            {
                this.statusWord &= 0xEFFF;
            }

            //Fault Reset
            if ((this.controlWord & 0x0080) > 0)
            {
                this.statusWord &= 0xFFBF;
            }

            //Halt
            if ((this.controlWord & 0x0100) > 0)
            {
            }
        }

        private void BuildPositionStatusWord()
        {
            throw new NotImplementedException();
        }

        private byte[] BuildRawActualPositionShaftMessage()
        {
            byte systemIndex;
            InverterParameterId parameterId;
            byte dataSet;

            lock (this.lastWriteMessage)
            {
                systemIndex = (byte)this.lastWriteMessage.SystemIndex;
                parameterId = this.lastWriteMessage.ParameterId;
                dataSet = this.lastWriteMessage.DataSetIndex;
            }

            //return (this.statusWord & 0x0002) > 0;
            //this.statusWord |= 0x0002;

            var rawMessage = new byte[10];

            rawMessage[0] = 0x00;
            rawMessage[1] = 0x08;
            rawMessage[2] = systemIndex;
            rawMessage[3] = dataSet;

            var parameterBytes = BitConverter.GetBytes((ushort)parameterId);

            rawMessage[4] = parameterBytes[0];
            rawMessage[5] = parameterBytes[1];

            var payloadBytes = BitConverter.GetBytes(++this.axisPosition);
            rawMessage[6] = payloadBytes[0];
            rawMessage[7] = payloadBytes[1];
            rawMessage[8] = payloadBytes[2];
            rawMessage[9] = payloadBytes[3];

            return rawMessage;
        }

        private byte[] BuildRawStatusMessage()
        {
            byte systemIndex;
            InverterParameterId parameterId;
            byte dataSet;

            lock (this.lastWriteMessage)
            {
                systemIndex = (byte)this.lastWriteMessage.SystemIndex;
                parameterId = this.lastWriteMessage.ParameterId;
                dataSet = this.lastWriteMessage.DataSetIndex;
            }

            var rawMessage = new byte[8];

            rawMessage[0] = 0x00;
            rawMessage[1] = 0x06;
            rawMessage[2] = systemIndex;
            rawMessage[3] = dataSet;

            var parameterBytes = BitConverter.GetBytes((ushort)parameterId);

            rawMessage[4] = parameterBytes[0];
            rawMessage[5] = parameterBytes[1];

            var payloadBytes = BitConverter.GetBytes(this.statusWord);
            rawMessage[6] = payloadBytes[0];
            rawMessage[7] = payloadBytes[1];

            return rawMessage;
        }

        private byte[] BuildRawStatusPowerOnMessage()
        {
            byte systemIndex;
            InverterParameterId parameterId;
            byte dataSet;

            lock (this.lastWriteMessage)
            {
                systemIndex = (byte)this.lastWriteMessage.SystemIndex;
                parameterId = this.lastWriteMessage.ParameterId;
                dataSet = this.lastWriteMessage.DataSetIndex;
            }

            //return (this.statusWord & 0x0002) > 0;
            //this.statusWord |= 0x0002;

            var rawMessage = new byte[8];

            rawMessage[0] = 0x00;
            rawMessage[1] = 0x06;
            rawMessage[2] = systemIndex;
            rawMessage[3] = dataSet;

            var parameterBytes = BitConverter.GetBytes((ushort)parameterId);

            rawMessage[4] = parameterBytes[0];
            rawMessage[5] = parameterBytes[1];

            var payloadBytes = BitConverter.GetBytes(this.statusWord);
            rawMessage[6] = payloadBytes[0];
            rawMessage[7] = payloadBytes[1];

            return rawMessage;
        }

        private byte[] BuildReadMessage(InverterMessage currentMessage)
        {
            byte[] returnValue = null;

            switch (currentMessage.ParameterId)
            {
                case InverterParameterId.DigitalInputsOutputs:
                    returnValue = this.BuildDigitalInputsMessage(currentMessage);
                    break;

                case InverterParameterId.StatusWord:
                    returnValue = this.BuildRawStatusPowerOnMessage();
                    break;

                case InverterParameterId.ActualPositionShaft:
                    returnValue = this.BuildRawActualPositionShaftMessage();
                    break;

                default:
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    break;
            }

            return returnValue;
        }

        private void BuildVelocityStatusWord()
        {
            //SwitchON
            if ((this.controlWord & 0x0001) > 0)
            {
                this.statusWord |= 0x0002;
            }
            else
            {
                this.statusWord &= 0xFFFD;
            }

            //EnableVoltage
            if ((this.controlWord & 0x0002) > 0)
            {
                this.statusWord |= 0x0001;
                this.statusWord |= 0x0010;
            }
            else
            {
                this.statusWord &= 0xFFFE;
                this.statusWord &= 0xFFEF;
            }

            //QuickStop
            if ((this.controlWord & 0x0004) > 0)
            {
                this.statusWord |= 0x0020;
            }
            else
            {
                this.statusWord &= 0xFFDF;
            }

            //EnableOperation
            if ((this.controlWord & 0x0008) > 0)
            {
                this.statusWord |= 0x0004;
                if (!this.targetTimerActive)
                {
                    this.targetTimer.Change(0, 1000);
                    this.targetTimerActive = true;
                }
            }
            else
            {
                this.statusWord &= 0xFFFB;
            }

            //Fault Reset
            if ((this.controlWord & 0x0080) > 0)
            {
                this.statusWord &= 0xFFBF;
            }

            //Halt
            if ((this.controlWord & 0x0100) > 0)
            {
            }
        }

        private void HomingTick(object state)
        {
            this.homingTickCount++;

            if (this.homingTickCount > 5)
            {
                this.statusWord |= 0x1000;
                this.homingTimerActive = false;
                this.homingTickCount = 0;
                this.homingTimer.Change(-1, Timeout.Infinite);
            }
        }

        private async Task<int> ParseReadMessage(InverterMessage inverterMessage, CancellationToken stoppingToken)
        {
            switch (inverterMessage.ParameterId)
            {
                case InverterParameterId.StatusWord:
                    return await this.ProcessStatusWordPayload(inverterMessage, stoppingToken);

                case InverterParameterId.DigitalInputsOutputs:
                    return await this.ProcessDigitalInputs(inverterMessage, stoppingToken);

                case InverterParameterId.ActualPositionShaft:
                    return await this.ProcessActualPositionShafts(inverterMessage, stoppingToken);

                default:
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    break;
            }
            return inverterMessage.GetReadMessage().Length;
        }

        private async Task<int> ParseWriteMessage(byte[] messageBytes, CancellationToken cancellationToken)
        {
            var message = InverterMessage.FromBytes(messageBytes);

            lock (this.lastWriteMessage)
            {
                this.lastWriteMessage = message;
            }

            if (message.IsWriteMessage)
            {
                return await this.ParseWriteMessage(message, cancellationToken);
            }

            return await this.ParseReadMessage(message, cancellationToken);
        }

        private async Task<int> ParseWriteMessage(InverterMessage message, CancellationToken stoppingToken)
        {
            switch (message.ParameterId)
            {
                case InverterParameterId.ControlWord:
                    return await this.ProcessControlWordPayload(message, stoppingToken);

                case InverterParameterId.SetOperatingMode:
                    return await this.ProcessSetOperatingModePayload(message, stoppingToken);

                case InverterParameterId.ShutterTargetPosition:
                    return await this.ProcessTargetPositionPayload(message, stoppingToken);

                case InverterParameterId.ShutterTargetVelocity:
                    return await this.ProcessTargetPositionPayload(message, stoppingToken);

                default:
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    break;
            }

            return message.ToBytes().Length;
        }

        private async Task<int> ProcessActualPositionShafts(InverterMessage inverterMessage, CancellationToken stoppingToken)
        {
            await Task.Delay(5, stoppingToken);

            lock (this.lastWriteMessage)
            {
                this.lastWriteMessage = inverterMessage;
            }

            this.readCompleteEventSlim.Set();

            return inverterMessage.GetReadMessage().Length;
        }

        private async Task<int> ProcessControlWordPayload(InverterMessage inverterMessage, CancellationToken stoppingToken)
        {
            this.controlWord = inverterMessage.UShortPayload;

            await Task.Delay(5, stoppingToken);

            this.readCompleteEventSlim.Set();

            return inverterMessage.ToBytes().Length;
        }

        private async Task<int> ProcessDigitalInputs(InverterMessage inverterMessage, CancellationToken stoppingToken)
        {
            await Task.Delay(5, stoppingToken);

            lock (this.lastWriteMessage)
            {
                this.lastWriteMessage = inverterMessage;
            }

            this.readCompleteEventSlim.Set();

            return inverterMessage.GetReadMessage().Length;
        }

        private async Task<int> ProcessSetOperatingModePayload(InverterMessage inverterMessage, CancellationToken stoppingToken)
        {
            this.operatingMode = Enum.Parse<InverterOperationMode>(inverterMessage.UShortPayload.ToString());

            await Task.Delay(5, stoppingToken);

            this.readCompleteEventSlim.Set();

            return inverterMessage.ToBytes().Length;
        }

        private async Task<int> ProcessStatusWordPayload(InverterMessage inverterMessage, CancellationToken stoppingToken)
        {
            await Task.Delay(5, stoppingToken);

            switch (this.operatingMode)
            {
                case InverterOperationMode.Homing:
                    this.BuildHomingStatusWord();
                    break;

                case InverterOperationMode.Position:
                    this.BuildPositionStatusWord();
                    break;

                case InverterOperationMode.Velocity:
                    this.BuildVelocityStatusWord();
                    break;

                case InverterOperationMode.ProfileVelocity:
                    this.BuildVelocityStatusWord();
                    break;

                default:
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    break;
            }
            this.readCompleteEventSlim.Set();

            return inverterMessage.GetReadMessage().Length;
        }

        private async Task<int> ProcessTargetPositionPayload(InverterMessage inverterMessage, CancellationToken stoppingToken)
        {
            //this.operatingMode = Enum.Parse<InverterOperationMode>(inverterMessage.UShortPayload.ToString());

            await Task.Delay(5, stoppingToken);

            this.readCompleteEventSlim.Set();

            return inverterMessage.ToBytes().Length;
        }

        private void TargetTick(object state)
        {
            this.targetTickCount++;

            if (this.targetTickCount > 10)
            {
                this.statusWord |= 0x0400;
                this.targetTimerActive = false;
                this.targetTimer.Change(-1, Timeout.Infinite);
                // Reset contatore
                this.targetTickCount = 0;
            }
        }

        #endregion
    }
}
