using System;
using System.IO;
using Ferretto.VW.MAS_IODriver.Enumerations;

namespace Ferretto.VW.MAS_IODriver
{
    public class IoSHDStatus
    {
        #region Fields

        private const int TOTAL_INPUTS = 16;

        private const int TOTAL_OUTPUTS = 8;

        private readonly byte errorCode;

        private readonly byte fwRelease;

        private readonly bool[] inputs;

        private readonly bool[] outputs;

        private SHDCodeOperation codeOperation;

        private short comTout;

        private byte debounceInput;

        private byte setupOutputLines;

        #endregion

        // is it useful??

        #region Constructors

        public IoSHDStatus()
        {
            this.inputs = new bool[TOTAL_INPUTS];
            this.outputs = new bool[TOTAL_OUTPUTS];
            this.fwRelease = 0x01;
            this.codeOperation = SHDCodeOperation.Data;  // 0x00: data, 0x01: configuration
            this.comTout = 500;
            this.setupOutputLines = 0x00;
            this.debounceInput = 0x32;
        }

        #endregion

        #region Properties

        public bool BayLightOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public SHDCodeOperation CodeOperation { get => this.codeOperation; set => this.codeOperation = value; }

        public short ComunicationTimeOut { get => this.comTout; set => this.comTout = value; }

        public bool CradleMotorOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public byte DebounceInput { get => this.debounceInput; set => this.debounceInput = value; }

        public bool ElevatorMotorOn => this.outputs?[(int)IoPorts.ElevatorMotor] ?? false;

        public bool MeasureBarrierOn => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public bool ResetSecurity => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public byte SetupOutputLines { get => this.setupOutputLines; set => this.setupOutputLines = value; }

        #endregion

        // Add other output signals names

        #region Methods

        public bool UpdateInputStates(bool[] newInputStates)
        {
            if (this.inputs.Length != newInputStates.Length)
            {
                throw new IOException($"Input states length mismatch while updating I/O driver status");
            }

            var updateRequired = false;
            for (var index = 0; index < TOTAL_INPUTS; index++)
            {
                if (this.inputs[index] != newInputStates[index])
                {
                    updateRequired = true;
                    break;
                }
            }
            try
            {
                if (updateRequired)
                {
                    Array.Copy(newInputStates, this.inputs, TOTAL_INPUTS);
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while updating Inputs status");
            }

            //return updateRequired;
            return true;
        }

        public bool UpdateOutputStates(bool[] newOutputStates)
        {
            var updateRequired = false;

            if (this.outputs.Length != newOutputStates.Length)
            {
                throw new IOException($"Output states length mismatch while updating I/O driver status");
            }

            for (var index = 0; index < TOTAL_OUTPUTS; index++)
            {
                if (this.outputs[index] != newOutputStates[index])
                {
                    this.outputs[index] = newOutputStates[index];
                    updateRequired = true;
                }
            }

            return updateRequired;
        }

        #endregion
    }
}
