using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Ferretto.VW.MAS.IODriver.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
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

        public const int TOTAL_INPUTS = 16;

        private const int TOTAL_OUTPUTS = 8;

        private readonly bool[] inputs;

        private readonly IoIndex ioIndex;

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

        public IoStatus(IoIndex ioIndex)
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
            this.ioIndex = ioIndex;
        }

        #endregion

        #region Properties

        [Column(Order = (int)IoPorts.AntiIntrusionBarrierBay)]
        public bool AntiIntrusionShutterBay => this.inputs?[(int)IoPorts.AntiIntrusionBarrierBay] ?? false;
        
        public bool BayLightOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;
        
        public short ComunicationTimeOut { get => this.comTout; set => this.comTout = value; }

        [Column(Order = (int)IoPorts.CradleMotor)]
        public bool CradleMotorOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        [Column(Order = (int)IoPorts.CradleMotorFeedback)]
        public bool CradleMotorSelected => this.inputs?[(int)IoPorts.CradleMotorFeedback] ?? false;

        public byte DebounceInput { get => this.debounceInput; set => this.debounceInput = value; }

        public bool ElevatorMotorOn => this.outputs?[(int)IoPorts.ElevatorMotor] ?? false;

        [Column(Order = (int)IoPorts.ElevatorMotorFeedback)]
        public bool ElevatorMotorSelected => this.inputs?[(int)IoPorts.ElevatorMotorFeedback] ?? false;

        public ShdFormatDataOperation FormatDataOperation { get => this.formatDataOperation; set => this.formatDataOperation = value; }

        public byte FwRelease { get => this.fwRelease; set => this.fwRelease = value; }

        public bool[] InputData => this.inputs;

        public IoIndex IoIndex => this.ioIndex;

        // Remove
        public string IpAddress { get => this.ipAddress; set => this.ipAddress = value; }

        [Column(Order = (int)IoPorts.LoadingUnitInBay)]
        public bool LoadingUnitExistenceInBay => this.inputs?[(int)IoPorts.LoadingUnitInBay] ?? false;
        
        public bool MeasureBarrierOn => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        [Column(Order = (int)IoPorts.MicroCarterLeftSideBay)]
        public bool MicroCarterLeftSideBay => this.inputs?[(int)IoPorts.MicroCarterLeftSideBay] ?? false;

        [Column(Order = (int)IoPorts.MicroCarterRightSideBay)]
        public bool MicroCarterRightSideBay => this.inputs?[(int)IoPorts.MicroCarterRightSideBay] ?? false;

        [Column(Order = (int)IoPorts.MushroomEmergency)]
        public bool MushroomEmergency => this.inputs?[(int)IoPorts.MushroomEmergency] ?? false;

        [Column(Order = (int)IoPorts.NormalState)]
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
