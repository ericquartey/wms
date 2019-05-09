using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Ferretto.VW.MAS_IODriver.Enumerations;

namespace Ferretto.VW.MAS_IODriver
{
    public class IoSHDMessage
    {
        #region Fields

        private const int N_CONFIG_BYTES = 8;

        private const int NBYTES = 12;

        private const byte RELEASE_PROTOCOL_01 = 0x01;

        private const int TOTAL_INPUTS = 16;

        private const int TOTAL_OUTPUTS = 8;

        private readonly SHDCodeOperation codeOperation;

        private readonly short comTout = 500;

        private readonly byte[] configurationData;

        private readonly byte debounceInput = 0x32;

        private readonly byte fwRelease;

        private readonly bool[] inputs;

        private readonly string ipAddress;

        private readonly bool[] outputs;

        private readonly byte setupOutputLines = 0x00;

        private readonly bool useSetupOutputLines = false;

        #endregion

        #region Constructors

        public IoSHDMessage()
        {
            this.Force = false;
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.fwRelease = RELEASE_PROTOCOL_01;
            this.codeOperation = SHDCodeOperation.Data;

            this.inputs = new bool[TOTAL_INPUTS];
            this.outputs = new bool[TOTAL_OUTPUTS];
        }

        public IoSHDMessage(bool read)
        {
            this.Force = false;
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.fwRelease = RELEASE_PROTOCOL_01;
            this.codeOperation = SHDCodeOperation.Data;

            if (read)
            {
                this.inputs = new bool[TOTAL_INPUTS];
            }
            else
            {
                this.outputs = new bool[TOTAL_OUTPUTS];
            }
        }

        public IoSHDMessage(int inputs, int outputs)
        {
            this.Force = false;
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.fwRelease = RELEASE_PROTOCOL_01;
            this.codeOperation = SHDCodeOperation.Data;

            if (inputs > 0)
            {
                this.inputs = new bool[inputs];
            }

            if (outputs > 0)
            {
                this.outputs = new bool[outputs];
            }
        }

        public IoSHDMessage(bool[] data, bool input)
        {
            this.Force = false;
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.fwRelease = RELEASE_PROTOCOL_01;
            this.codeOperation = SHDCodeOperation.Data;

            if (input)
            {
                this.inputs = new bool[data.Length];
                try
                {
                    Array.Copy(data, this.inputs, data.Length);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Exception {ex.Message} while initializing Inputs status");
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
                    throw new IOException($"Exception {ex.Message} while initializing Outputs status");
                }
            }
        }

        public IoSHDMessage(bool[] inputs, bool[] outputs)
        {
            this.Force = false;

            this.configurationData = new byte[N_CONFIG_BYTES];
            this.fwRelease = RELEASE_PROTOCOL_01;
            this.codeOperation = SHDCodeOperation.Data;

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

        public IoSHDMessage(byte[] configurationData)
        {
            this.Force = false;

            this.configurationData = new byte[N_CONFIG_BYTES];
            this.fwRelease = RELEASE_PROTOCOL_01;
            this.codeOperation = SHDCodeOperation.Configuration;

            this.inputs = new bool[TOTAL_INPUTS];
            this.outputs = new bool[TOTAL_OUTPUTS];

            try
            {
                Array.Copy(configurationData, this.configurationData, configurationData.Length);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while initializing Inputs status");
            }

            this.comTout = (short)(this.configurationData[0] + (this.configurationData[1] << 8));
            this.useSetupOutputLines = (this.configurationData[2] == 0);
            this.setupOutputLines = this.configurationData[3];
            this.debounceInput = this.configurationData[4];
        }

        public IoSHDMessage(short comTout, bool useSetupOutputLines, byte setupOutputLines, byte debounceInput)
        {
            this.Force = false;

            this.configurationData = new byte[N_CONFIG_BYTES];
            this.fwRelease = RELEASE_PROTOCOL_01;
            this.codeOperation = SHDCodeOperation.Configuration;

            this.inputs = new bool[TOTAL_INPUTS];
            this.outputs = new bool[TOTAL_OUTPUTS];

            // TODO Check arguments

            this.comTout = comTout;
            this.useSetupOutputLines = useSetupOutputLines;
            this.setupOutputLines = setupOutputLines;
            this.debounceInput = debounceInput;

            var bytes = BitConverter.GetBytes(this.comTout);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Array.Copy(bytes, 0, this.configurationData, 0, sizeof(short));
            this.configurationData[2] = (this.useSetupOutputLines) ? (byte)0x00 : (byte)0x01;
            this.configurationData[3] = this.setupOutputLines;
            this.configurationData[4] = this.debounceInput;
        }

        public IoSHDMessage(string ipAddress)
        {
            this.Force = false;

            this.configurationData = new byte[N_CONFIG_BYTES];
            this.fwRelease = RELEASE_PROTOCOL_01;
            this.codeOperation = SHDCodeOperation.SetIP;
            this.ipAddress = ipAddress;

            this.inputs = new bool[TOTAL_INPUTS];
            this.outputs = new bool[TOTAL_OUTPUTS];

            if (ipAddress != "")
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

        public SHDCodeOperation CodeOperation => this.codeOperation;

        public short ComunicationTimeOut => this.comTout;

        public byte[] ConfigurationData => this.configurationData;

        public bool CradleMotorOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public byte DebounceInput => this.debounceInput;

        public bool ElevatorMotorOn => this.outputs?[(int)IoPorts.ElevatorMotor] ?? false;

        public bool Force { get; set; }

        public byte FwRelease => this.fwRelease;

        public bool[] Inputs => this.inputs;

        public string IpAddress => this.ipAddress;

        public bool MeasureBarrierOn => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public bool[] Outputs => this.outputs;

        public bool OutputsCleared => !this.outputs?.Any(o => o) ?? false;

        public bool ResetSecurity => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public byte SetupOutputLines => this.setupOutputLines;

        public bool UseSetupOutputLines => this.useSetupOutputLines;

        public bool ValidInputs => this.inputs != null;

        public bool ValidOutputs => this.outputs != null;

        #endregion

        #region Methods

        // To be removed!!!
        public IoMessage GetIoMessage()
        {
            var message = new IoMessage(this.inputs, this.outputs);
            message.Force = this.Force;
            return message;
        }

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
            telegram[1] = this.fwRelease;
            // Code op
            switch (this.codeOperation)
            {
                case SHDCodeOperation.Data:
                    telegram[2] = (byte)0x00;
                    break;

                case SHDCodeOperation.Configuration:
                    telegram[2] = (byte)0x01;
                    break;

                case SHDCodeOperation.SetIP:
                    telegram[2] = (byte)0x02;
                    break;

                default:
                    telegram[2] = (byte)0x00;
                    break;
            }

            switch (fwRelease)
            {
                case 0x10:
                    // Payload output
                    telegram[3] = this.BoolArrayToByte(this.outputs);

                    // Configuration data
                    Array.Copy(telegram, 4, this.configurationData, 0, this.configurationData.Length);

                    break;

                case 0x11:

                    // Alignment
                    telegram[3] = 0x00;

                    // Payload output
                    telegram[4] = this.BoolArrayToByte(this.outputs);

                    // Configuration data
                    Array.Copy(telegram, 5, this.configurationData, 0, this.configurationData.Length);

                    break;

                default:
                    // Payload output
                    telegram[3] = this.BoolArrayToByte(this.outputs);

                    // Configuration data
                    Array.Copy(telegram, 4, this.configurationData, 0, this.configurationData.Length);
                    break;
            }

            return telegram;
        }

        public bool SwitchCradleMotor(bool switchOn)
        {
            if (this.outputs == null)
            {
                throw new ArgumentNullException(nameof(this.Outputs), "Message Digital Outputs are not initialized correctly");
            }

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
            if (this.outputs == null)
            {
                throw new ArgumentNullException(nameof(this.Outputs), "Message Digital Outputs are not initialized correctly");
            }

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
            if (this.outputs == null)
            {
                throw new ArgumentNullException(nameof(this.Outputs), "Message Digital Outputs are not initialized correctly");
            }

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

        private byte BoolArrayToByte(bool[] b)
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
