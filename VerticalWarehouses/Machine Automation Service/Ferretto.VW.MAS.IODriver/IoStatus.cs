using System;
using System.IO;
using Ferretto.VW.MAS.IODriver.Enumerations;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.IODriver
{
    public class IoStatus
    {

        #region Fields

        private const short COMTOUT_DEFAULT = 20000;

        private const byte DEBOUNCE_INPUT_DEFAULT = 0x23; // 35 ms

        private const byte RELEASE_FW_10 = 0x10;

        private const byte RELEASE_FW_11 = 0x11;

        private const byte SETUP_OUTPUTLINES_DEFAULT = 0x00;

        private const int TOTAL_INPUTS = 16;

        private const int TOTAL_OUTPUTS = 8;

        private readonly bool[] inputs;

        private readonly bool[] outputs;

        private short comTout;

        private byte debounceInput;

        private ShdFormatDataOperation formatDataOperation;

        private byte fwRelease;

        private string ipAddress;

        private byte setupOutputLines;

        private bool useSetupOutputLines;

        #endregion

        #region Constructors

        public IoStatus()
        {
            this.inputs = new bool[TOTAL_INPUTS];
            this.outputs = new bool[TOTAL_OUTPUTS];

            this.fwRelease = RELEASE_FW_10;
            this.formatDataOperation = ShdFormatDataOperation.Data;
            this.comTout = COMTOUT_DEFAULT;
            this.setupOutputLines = SETUP_OUTPUTLINES_DEFAULT;
            this.debounceInput = DEBOUNCE_INPUT_DEFAULT;
            this.useSetupOutputLines = false;
            this.ipAddress = string.Empty;
        }

        #endregion



        #region Properties

        public bool AntiIntrusionShutterBay => this.inputs?[(int)IoPorts.AntiIntrusionBarrierBay] ?? false;

        public bool BayLightOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public short ComunicationTimeOut { get => this.comTout; set => this.comTout = value; }

        public bool CradleMotorOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public bool CradleMotorSelected => this.inputs?[(int)IoPorts.CradleMotorFeedback] ?? false;

        public byte DebounceInput { get => this.debounceInput; set => this.debounceInput = value; }

        public bool ElevatorMotorOn => this.outputs?[(int)IoPorts.ElevatorMotor] ?? false;

        public bool ElevatorMotorSelected => this.inputs?[(int)IoPorts.ElevatorMotorFeedback] ?? false;

        public ShdFormatDataOperation FormatDataOperation { get => this.formatDataOperation; set => this.formatDataOperation = value; }

        public byte FwRelease { get => this.fwRelease; set => this.fwRelease = value; }

        public bool[] InputData => this.inputs;

        // Remove
        public string IpAddress { get => this.ipAddress; set => this.ipAddress = value; }

        public bool LoadingUnitExistenceInBay => this.inputs?[(int)IoPorts.LoadingUnitInBay] ?? false;

        public bool MeasureBarrierOn => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public bool MicroCarterLeftSideBay => this.inputs?[(int)IoPorts.MicroCarterLeftSideBay] ?? false;

        public bool MicroCarterRightSideBay => this.inputs?[(int)IoPorts.MicroCarterRightSideBay] ?? false;

        public bool MushroomEmergency => this.inputs?[(int)IoPorts.MushroomEmergency] ?? false;

        public bool NormalState => this.inputs?[(int)IoPorts.NormalState] ?? false;

        public bool[] OutputData => this.outputs;

        public bool ResetSecurity => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public byte SetupOutputLines { get => this.setupOutputLines; set => this.setupOutputLines = value; }

        public bool UseSetupOutputLines { get => this.useSetupOutputLines; set => this.useSetupOutputLines = value; }

        #endregion

        // Add other output signals names



        #region Methods

        public bool MatchOutputs(bool[] outputsState)
        {
            var matched = true;
            for (var index = 0; index < TOTAL_OUTPUTS; index++)
            {
                if (this.outputs[index] != outputsState[index])
                {
                    matched = false;
                }
            }
            return matched;
        }

        public bool UpdateInputStates(bool[] newInputStates)
        {
            if (this.inputs.Length != newInputStates.Length)
            {
                throw new IOException($"Input states length mismatch while updating I/O driver status");
            }

            var changeValues = false;
            for (var i = 0; i < this.inputs.Length; i++)
            {
                if (this.inputs[i] != newInputStates[i])
                {
                    changeValues = true;
                }
            }

            try
            {
                Array.Copy(newInputStates, this.inputs, TOTAL_INPUTS);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while updating Inputs status");
            }

            return changeValues;
        }

        public bool UpdateOutputStates(bool[] newOutputStates)
        {
            if (this.outputs.Length != newOutputStates.Length)
            {
                throw new IOException($"Output states length mismatch while updating I/O driver status");
            }

            for (var index = 0; index < TOTAL_OUTPUTS; index++)
            {
                if (this.outputs[index] != newOutputStates[index])
                {
                    this.outputs[index] = newOutputStates[index];
                }
            }

            return true;
        }

        public bool UpdateSetupParameters(short comTout, byte debounceInput, byte setupOutputLines, bool useSetupOutputLines, string ipAddress)
        {
            this.comTout = comTout;
            this.useSetupOutputLines = useSetupOutputLines;
            this.debounceInput = debounceInput;
            this.setupOutputLines = setupOutputLines;
            this.ipAddress = ipAddress;

            return true;
        }

        public bool UpdateStates(bool[] newInputStates, bool[] newOutputStates)
        {
            if (this.inputs.Length != newInputStates.Length)
            {
                throw new IOException($"Input states length mismatch while updating I/O driver status");
            }
            if (this.outputs.Length != newOutputStates.Length)
            {
                throw new IOException($"Output states length mismatch while updating I/O driver status");
            }

            try
            {
                Array.Copy(newInputStates, this.inputs, TOTAL_INPUTS);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while updating Inputs status");
            }

            for (var index = 0; index < TOTAL_OUTPUTS; index++)
            {
                if (this.outputs[index] != newOutputStates[index])
                {
                    this.outputs[index] = newOutputStates[index];
                }
            }

            return true;
        }

        #endregion
    }
}
