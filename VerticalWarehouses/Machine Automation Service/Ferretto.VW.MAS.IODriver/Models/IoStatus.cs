using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.IODriver
{
    public class IoStatus
    {
        #region Fields

        public const int TotalInputs = 16;

        private const short COMTOUT_DEFAULT = 2000;

        /// <summary>
        /// Corresponds to 50ms.
        /// </summary>
        private const byte DEBOUNCE_INPUT_DEFAULT = 0x32;

        private const byte RELEASE_FW_10 = 0x10;

        private const byte RELEASE_FW_11 = 0x11;

        private const byte SETUP_OUTPUTLINES_DEFAULT = 0x00;

        private const int TotalOutputs = 8;

        private readonly int[] diagOutCurrent = new int[TotalOutputs];

        private readonly bool[] diagOutFault = new bool[TotalOutputs];

        private readonly bool[] inputs = new bool[TotalInputs];

        private readonly bool[] outputs = new bool[TotalOutputs];

        private short comTout;

        #endregion

        #region Constructors

        public IoStatus(IoIndex ioIndex)
        {
            this.FwRelease = RELEASE_FW_10;
            this.FormatDataOperation = ShdFormatDataOperation.Data;
            this.comTout = COMTOUT_DEFAULT;
            this.SetupOutputLines = SETUP_OUTPUTLINES_DEFAULT;
            this.DebounceInput = DEBOUNCE_INPUT_DEFAULT;
            this.UseSetupOutputLines = false;
            this.IoIndex = ioIndex;
        }

        #endregion

        #region Properties

        [Column(Order = (int)IoPorts.AntiIntrusionBarrierBay)]
        public bool AntiIntrusionShutterBay => this.inputs[(int)IoPorts.AntiIntrusionBarrierBay];

        public bool BayLightOn => this.outputs[(int)IoPorts.CradleMotor];

        public short ComunicationTimeOut { get => this.comTout; set => this.comTout = value; }

        [Column(Order = (int)IoPorts.CradleMotor)]
        public bool CradleMotorOn => this.outputs[(int)IoPorts.CradleMotor];

        [Column(Order = (int)IoPorts.CradleMotorFeedback)]
        public bool CradleMotorSelected => this.inputs[(int)IoPorts.CradleMotorFeedback];

        public byte DebounceInput { get; set; }

        public bool ElevatorMotorOn => this.outputs[(int)IoPorts.ElevatorMotor];

        [Column(Order = (int)IoPorts.ElevatorMotorFeedback)]
        public bool ElevatorMotorSelected => this.inputs[(int)IoPorts.ElevatorMotorFeedback];

        [Column(Order = (int)IoPorts.FinePickingRobot)]
        public bool FireAlarm => this.inputs[(int)IoPorts.FinePickingRobot];

        public ShdFormatDataOperation FormatDataOperation { get; set; }

        public byte FwRelease { get; set; }

        public bool[] InputData => this.inputs;

        public IoIndex IoIndex { get; }

        [Column(Order = (int)IoPorts.LoadingUnitInBay)]
        public bool LoadingUnitExistenceInBay => this.inputs[(int)IoPorts.LoadingUnitInBay];

        public bool MeasureProfileOn => this.outputs[(int)IoPorts.MeasureProfile];

        [Column(Order = (int)IoPorts.MicroCarterLeftSideBay)]
        public bool MicroCarterLeftSideBay => this.inputs[(int)IoPorts.MicroCarterLeftSideBay];

        [Column(Order = (int)IoPorts.MicroCarterRightSideBay)]
        public bool MicroCarterRightSideBay => this.inputs[(int)IoPorts.MicroCarterRightSideBay];

        [Column(Order = (int)IoPorts.MushroomEmergency)]
        public bool MushroomEmergency => this.inputs[(int)IoPorts.MushroomEmergency];

        [Column(Order = (int)IoPorts.NormalState)]
        public bool NormalState => this.inputs[(int)IoPorts.NormalState];

        public bool[] OutputData => this.outputs;

        [Column(Order = (int)IoPorts.HookTrolley)]
        public bool PreFireAlarm => this.inputs[(int)IoPorts.HookTrolley];

        public bool ResetSecurity => this.outputs[(int)IoPorts.ResetSecurity];

        public byte SetupOutputLines { get; set; }

        public bool UseSetupOutputLines { get; set; }

        #endregion

        #region Methods

        public bool MatchOutputs(bool[] outputsState)
        {
            if (outputsState is null)
            {
                throw new ArgumentNullException(nameof(outputsState));
            }

            if (outputsState.Length != this.outputs.Length)
            {
                throw new ArgumentException($"{nameof(outputsState)} length should match {this.outputs} length", nameof(outputsState));
            }

            for (var index = 0; index < this.outputs.Length; index++)
            {
                if (this.outputs[index] != outputsState[index])
                {
                    return false;
                }
            }

            return true;
        }

        public bool UpdateInputStates(bool[] newStates)
        {
            if (newStates is null)
            {
                throw new ArgumentNullException(nameof(newStates));
            }

            if (this.inputs.Length != newStates.Length)
            {
                throw new IOException($"Input states length mismatch while updating I/O driver status");
            }

            var changeValues = false;
            for (var i = 0; i < newStates.Length; i++)
            {
                if (this.inputs[i] != newStates[i])
                {
                    changeValues = true;
                }
            }

            try
            {
                Array.Copy(newStates, this.inputs, TotalInputs);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while updating Inputs status");
            }

            return changeValues;
        }

        public bool UpdateOutCurrentStates(int[] newStates)
        {
            if (newStates is null)
            {
                throw new ArgumentNullException(nameof(newStates));
            }

            if (this.diagOutCurrent.Length != newStates.Length)
            {
                throw new IOException($"Output current states length mismatch while updating I/O driver status");
            }

            var changeValues = false;
            for (var i = 0; i < newStates.Length; i++)
            {
                if (Math.Abs(this.diagOutCurrent[i] - newStates[i]) > 2)
                {
                    changeValues = true;
                }
            }

            try
            {
                Array.Copy(newStates, this.diagOutCurrent, newStates.Length);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while updating output current status");
            }

            return changeValues;
        }

        public bool UpdateOutFaultStates(bool[] newStates)
        {
            if (newStates is null)
            {
                throw new ArgumentNullException(nameof(newStates));
            }

            if (this.diagOutFault.Length != newStates.Length)
            {
                throw new IOException($"Output fault states length mismatch while updating I/O driver status");
            }

            var changeValues = false;
            for (var i = 0; i < newStates.Length; i++)
            {
                if (this.diagOutFault[i] != newStates[i])
                {
                    changeValues = true;
                }
            }

            try
            {
                Array.Copy(newStates, this.diagOutFault, newStates.Length);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while updating output fault status");
            }

            return changeValues;
        }

        public void UpdateOutputStates(bool[] newOutputStates)
        {
            if (newOutputStates is null)
            {
                throw new ArgumentNullException(nameof(newOutputStates));
            }

            if (this.outputs.Length != newOutputStates.Length)
            {
                throw new IOException($"Output states length mismatch while updating I/O driver status");
            }

            Array.Copy(newOutputStates, this.outputs, this.outputs.Length);
        }

        #endregion
    }
}
