using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Ferretto.VW.MAS.IODriver
{
    internal class IoMessage
    {
        #region Fields

        private const int N_CONFIG_BYTES = 8;

        private const int NBYTES = 12;

        private const int TotalInputs = 16;

        private const int TotalOutputs = 8;

        private readonly ShdCodeOperation codeOperation;

        private readonly short comTimeout = 20000;

        private readonly byte[] configurationData;

        private readonly byte debounceInput = 0x32;

        private readonly bool[] inputs;

        private readonly string ipAddress;

        private readonly bool[] outputs;

        private readonly byte setupOutputLines = 0x00;

        private readonly bool useSetupOutputLines = false;

        #endregion

        #region Constructors

        public IoMessage()
        {
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.codeOperation = ShdCodeOperation.Data;

            this.inputs = new bool[TotalInputs];
            this.outputs = new bool[TotalOutputs];
        }

        public IoMessage(bool read)
        {
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.codeOperation = ShdCodeOperation.Data;

            if (read)
            {
                this.inputs = new bool[TotalInputs];
            }
            else
            {
                this.outputs = new bool[TotalOutputs];
            }
        }

        public IoMessage(int inputs, int outputs)
        {
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.codeOperation = ShdCodeOperation.Data;

            if (inputs > 0)
            {
                this.inputs = new bool[inputs];
            }

            if (outputs > 0)
            {
                this.outputs = new bool[outputs];
            }
        }

        public IoMessage(ShdCodeOperation codeOperation)
        {
            this.codeOperation = codeOperation;

            this.configurationData = new byte[N_CONFIG_BYTES];
            this.inputs = new bool[TotalInputs];
            this.outputs = new bool[TotalOutputs];
        }

        public IoMessage(bool[] data, bool input)
        {
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.codeOperation = ShdCodeOperation.Data;

            if (input)
            {
                this.inputs = new bool[data.Length];
                try
                {
                    Array.Copy(data, this.inputs, data.Length);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Error while initializing I/O input message.", ex);
                }
            }
            else
            {
                this.outputs = new bool[data.Length];
                try
                {
                    Array.Copy(data, this.outputs, data.Length);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Error while initializing I/O output message.", ex);
                }
            }
        }

        public IoMessage(bool[] inputs, bool[] outputs)
        {
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.codeOperation = ShdCodeOperation.Data;

            this.inputs = new bool[inputs.Length];
            this.outputs = new bool[outputs.Length];

            try
            {
                Array.Copy(inputs, this.inputs, inputs.Length);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while initializing Inputs status");
            }

            try
            {
                Array.Copy(outputs, this.outputs, outputs.Length);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while initializing Outputs status");
            }
        }

        public IoMessage(byte[] configurationData)
        {
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.codeOperation = ShdCodeOperation.Configuration;

            this.inputs = new bool[TotalInputs];
            this.outputs = new bool[TotalOutputs];

            try
            {
                Array.Copy(configurationData, this.configurationData, configurationData.Length);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while initializing Inputs status");
            }

            this.comTimeout = (short)(this.configurationData[0] + (this.configurationData[1] << 8));
            this.useSetupOutputLines = this.configurationData[2] == 0;
            this.setupOutputLines = this.configurationData[3];
            this.debounceInput = this.configurationData[4];
        }

        public IoMessage(short comTout, bool useSetupOutputLines, byte setupOutputLines, byte debounceInput)
        {
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.codeOperation = ShdCodeOperation.Configuration;

            this.inputs = new bool[TotalInputs];
            this.outputs = new bool[TotalOutputs];

            // TODO Check arguments
            this.comTimeout = comTout;
            this.useSetupOutputLines = useSetupOutputLines;
            this.setupOutputLines = setupOutputLines;
            this.debounceInput = debounceInput;

            var bytes = BitConverter.GetBytes(this.comTimeout);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            Array.Copy(bytes, 0, this.configurationData, 0, sizeof(short));
            this.configurationData[2] = this.useSetupOutputLines ? (byte)0x00 : (byte)0x01;
            this.configurationData[3] = this.setupOutputLines;
            this.configurationData[4] = this.debounceInput;
        }

        public IoMessage(string ipAddress)
        {
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.codeOperation = ShdCodeOperation.SetIP;
            this.ipAddress = ipAddress;

            this.inputs = new bool[TotalInputs];
            this.outputs = new bool[TotalOutputs];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                var address = IPAddress.Parse(ipAddress);
                var bytes = address.GetAddressBytes();
                Array.Reverse(bytes);
                Array.Copy(bytes, 0, this.configurationData, 0, 4);
            }
        }

        #endregion

        #region Properties

        public bool BayLightOn => this.outputs?[(int)IoPorts.BayLight] ?? false;

        public ShdCodeOperation CodeOperation => this.codeOperation;

        public short ComunicationTimeOut => this.comTimeout;

        public byte[] ConfigurationData => this.configurationData;

        public bool CradleMotorOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public byte DebounceInput => this.debounceInput;

        public bool ElevatorMotorOn => this.outputs?[(int)IoPorts.ElevatorMotor] ?? false;

        public bool EndMissionRobotOn => this.outputs?[(int)IoPorts.EndMissionRobot] ?? false;

        public bool[] Inputs => this.inputs;

        public string IpAddress => this.ipAddress;

        public bool MeasureProfileOn => this.outputs?[(int)IoPorts.MeasureProfile] ?? false;

        public bool[] Outputs => this.outputs;

        public bool OutputsCleared => !this.outputs?.Any(o => o) ?? false;

        public bool PowerEnable => this.outputs?[(int)IoPorts.PowerEnable] ?? false;

        public bool ReadyWarehouseRobotOn => this.outputs?[(int)IoPorts.ReadyWarehouseRobot] ?? false;

        public bool ResetSecurity => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public byte SetupOutputLines => this.setupOutputLines;

        public bool UseSetupOutputLines => this.useSetupOutputLines;

        public bool ValidInputs => this.inputs != null;

        public bool ValidOutputs => this.outputs != null;

        #endregion

        #region Methods

        /// <summary>
        /// Get the telegram to send to RemoteIO device.
        /// </summary>
        public byte[] GetWriteTelegramBytes(byte fwRelease)
        {
            // check argument
            var nBytesToSend = NBYTES;
            switch (fwRelease)
            {
                case 0x10:
                    break;

                case 0x11:
                    nBytesToSend = NBYTES + 1;
                    break;

                default:
                    break;
            }

            // Create telegram to send
            var telegram = new byte[nBytesToSend];

            // nBytes
            telegram[0] = (byte)nBytesToSend;

            // Fw release
            telegram[1] = fwRelease;

            // Code op
            switch (this.codeOperation)
            {
                case ShdCodeOperation.Data:
                    telegram[2] = 0x00;
                    break;

                case ShdCodeOperation.Configuration:
                    telegram[2] = 0x01;
                    break;

                case ShdCodeOperation.SetIP:
                    telegram[2] = 0x02;
                    break;

                default:
                    telegram[2] = 0x00;
                    break;
            }

            switch (fwRelease)
            {
                case 0x10:
                    // Payload output
                    telegram[3] = BoolArrayToByte(this.outputs);

                    // Configuration data
                    Array.Copy(telegram, 4, this.configurationData, 0, this.configurationData.Length);

                    break;

                case 0x11:

                    // Alignment
                    telegram[3] = 0x00;

                    // Payload output
                    telegram[4] = BoolArrayToByte(this.outputs);

                    // Configuration data
                    Array.Copy(telegram, 5, this.configurationData, 0, this.configurationData.Length);

                    break;

                default:
                    // Payload output
                    telegram[3] = BoolArrayToByte(this.outputs);

                    // Configuration data
                    Array.Copy(telegram, 4, this.configurationData, 0, this.configurationData.Length);
                    break;
            }

            return telegram;
        }

        public bool SwitchCradleMotor(bool switchOn)
        {
            Trace.Assert(this.outputs == null, "Message Digital Outputs are not initialized correctly");

            if (switchOn)
            {
                if (this.outputs[(int)IoPorts.ElevatorMotor])
                {
                    return false;
                }

                this.outputs[(int)IoPorts.CradleMotor] = true;
            }
            else
            {
                this.outputs[(int)IoPorts.CradleMotor] = false;
            }

            return true;
        }

        public bool SwitchElevatorMotor(bool switchOn)
        {
            Trace.Assert(this.outputs == null, "Message Digital Outputs are not initialized correctly");

            if (switchOn)
            {
                if (this.outputs[(int)IoPorts.CradleMotor])
                {
                    return false;
                }

                this.outputs[(int)IoPorts.ElevatorMotor] = true;
            }
            else
            {
                this.outputs[(int)IoPorts.ElevatorMotor] = false;
            }

            return true;
        }

        public bool SwitchResetSecurity(bool switchOn)
        {
            Trace.Assert(this.outputs == null, "Message Digital Outputs are not initialized correctly");

            this.outputs[(int)IoPorts.ResetSecurity] = switchOn;

            return true;
        }

        public override string ToString()
        {
            var returnString = new StringBuilder();

            returnString.Append("IoMessage:I[");

            for (var i = 0; i < this.inputs?.Length; i++)
            {
                returnString.Append(string.Format("{0}.", this.inputs[i] ? "T" : "F"));
            }

            returnString.Append("]-O[");

            for (var i = 0; i < this.outputs?.Length; i++)
            {
                returnString.Append(string.Format("{0}.", this.outputs[i] ? "T" : "F"));
            }

            returnString.Append("]");

            return returnString.ToString();
        }

        private static byte BoolArrayToByte(bool[] b)
        {
            const int N_BITS_8 = 8;
            var value = 0x00;
            for (var i = 0; i < N_BITS_8; i++)
            {
                value += b[i] ? 1 : 0;
            }

            return Convert.ToByte(value);
        }

        #endregion
    }
}
