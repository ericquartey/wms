using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Ferretto.VW.MAS.IODriver
{
    internal class IoWriteMessage
    {
        #region Fields

        private const int N_CONFIG_BYTES = 8;

        private const int NBYTES = 12;

        private const byte RELEASE_PROTOCOL_01 = 0x01;

        private const int TotalOutputs = 8;

        private readonly ShdCodeOperation codeOperation;

        private readonly short comTout = 20000; // 20 s     // Time out

        private readonly byte[] configurationData;

        private readonly byte debounceInput = 0x32;

        private readonly bool[] outputs;

        private readonly byte setupOutputLines = 0x00;

        private readonly bool useSetupOutputLines = false;

        #endregion

        #region Constructors

        public IoWriteMessage()
        {
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.codeOperation = ShdCodeOperation.Data;

            this.outputs = new bool[TotalOutputs];
        }

        public IoWriteMessage(bool[] outputs)
        {
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.codeOperation = ShdCodeOperation.Data;

            this.outputs = new bool[outputs.Length];
            try
            {
                Array.Copy(outputs, this.outputs, outputs.Length);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while initializing Outputs status");
            }
        }

        public IoWriteMessage(short comTout, bool useSetupOutputLines, byte setupOutputLines, byte debounceInput)
        {
            this.configurationData = new byte[N_CONFIG_BYTES];
            this.codeOperation = ShdCodeOperation.Configuration;

            this.outputs = new bool[TotalOutputs];

            // TODO Check arguments
            this.comTout = comTout;
            this.useSetupOutputLines = useSetupOutputLines;
            this.setupOutputLines = setupOutputLines;
            this.debounceInput = debounceInput;

            var bytes = BitConverter.GetBytes(this.comTout);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            Array.Copy(bytes, 0, this.configurationData, 0, sizeof(short));
            this.configurationData[2] = this.useSetupOutputLines ? (byte)0x01 : (byte)0x00;
            this.configurationData[3] = this.setupOutputLines;
            this.configurationData[4] = this.debounceInput;
        }

        #endregion

        #region Properties

        public bool BayLightOn
        {
            get => this.outputs?[(int)IoPorts.BayLight] ?? false;
            set => this.outputs[(int)IoPorts.BayLight] = value;
        }

        public ShdCodeOperation CodeOperation => this.codeOperation;

        public short ComunicationTimeOut => this.comTout;

        public byte[] ConfigurationData => this.configurationData;

        public bool CradleMotorOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public byte DebounceInput => this.debounceInput;

        public bool ElevatorMotorOn => this.outputs?[(int)IoPorts.ElevatorMotor] ?? false;

        public bool EndMissionRobotOn
        {
            get => this.outputs?[(int)IoPorts.EndMissionRobot] ?? false;
            set => this.outputs[(int)IoPorts.EndMissionRobot] = value;
        }

        public bool MeasureProfileOn => this.outputs?[(int)IoPorts.MeasureProfile] ?? false;

        public bool[] Outputs => this.outputs;

        public bool OutputsCleared => !this.outputs?.Any(o => o) ?? false;

        public bool PowerEnable
        {
            get => this.outputs[(int)IoPorts.PowerEnable];
            set => this.outputs[(int)IoPorts.PowerEnable] = value;
        }

        public bool ReadyWarehouseRobotOn
        {
            get => this.outputs?[(int)IoPorts.ReadyWarehouseRobot] ?? false;
            set => this.outputs[(int)IoPorts.ReadyWarehouseRobot] = value;
        }

        public bool ResetSecurity
        {
            get => this.outputs[(int)IoPorts.ResetSecurity];
            set => this.outputs[(int)IoPorts.ResetSecurity] = value;
        }

        public byte SetupOutputLines => this.setupOutputLines;

        public bool UseSetupOutputLines => this.useSetupOutputLines;

        #endregion

        #region Methods

        /// <summary>
        /// Build the telegram to send to RemoteIO device.
        /// </summary>
        public byte[] BuildSendTelegram(byte fwRelease)
        {
            // check argument
            var nBytesToSend = NBYTES;
            switch (fwRelease)
            {
                case 0x10:
                    break;

                case 0x11:
                case 0x12: // new release
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
            telegram[1] = RELEASE_PROTOCOL_01;

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
                    Array.Copy(this.configurationData, 0, telegram, 4, this.configurationData.Length);

                    break;

                case 0x11:
                case 0x12: // new release

                    // Alignment
                    telegram[3] = 0x00;

                    // Payload output
                    telegram[4] = BoolArrayToByte(this.outputs);

                    // Configuration data
                    Array.Copy(this.configurationData, 0, telegram, 5, this.configurationData.Length);

                    break;

                default:
                    // Payload output
                    telegram[3] = BoolArrayToByte(this.outputs);

                    // Configuration data
                    Array.Copy(this.configurationData, 0, telegram, 4, this.configurationData.Length);
                    break;
            }

            return telegram;
        }

        public bool SwitchBayLight(bool lightOn)
        {
            this.outputs[(int)IoPorts.BayLight] = lightOn;

            return true;
        }

        public bool SwitchCradleMotor(bool switchOn)
        {
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

        public bool SwitchEndMissionRobot(bool value)
        {
            this.outputs[(int)IoPorts.EndMissionRobot] = value;
            return true;
        }

        public bool SwitchMeasureProfile(bool switchOn)
        {
            this.outputs[(int)IoPorts.MeasureProfile] = switchOn;

            return true;
        }

        public bool SwitchReadyWarehouseRobot(bool value)
        {
            this.outputs[(int)IoPorts.ReadyWarehouseRobot] = value;
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

            returnString.Append(" O[");

            for (var i = 0; i < this.outputs?.Length; i++)
            {
                returnString.Append(string.Format("{0}.", this.outputs[i] ? "T" : "F"));
            }

            returnString.Append("]");

            return returnString.ToString();
        }

        private static byte BoolArrayToByte(bool[] b)
        {
            byte value = 0x00;
            var index = 0;
            foreach (var el in b)
            {
                if (el)
                {
                    value |= (byte)(1 << index);
                }

                index++;
            }

            return value;
        }

        #endregion
    }
}
