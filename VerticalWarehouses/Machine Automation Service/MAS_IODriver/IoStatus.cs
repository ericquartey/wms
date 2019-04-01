using System;
using System.IO;
using Ferretto.VW.MAS_IODriver.Enumerations;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver
{
    public class IoStatus
    {
        #region Fields

        private const int TOTAL_INPUTS = 8;

        private const int TOTAL_OUTPUTS = 5;

        private bool[] inputs;

        private bool[] outputs;

        #endregion

        #region Constructors

        public IoStatus()
        {
            this.inputs = new bool[TOTAL_INPUTS];
            this.outputs = new bool[TOTAL_OUTPUTS];
        }

        #endregion

        #region Properties

        public bool BayLightOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public bool CradleMotorOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public bool ElevatorMotorOn => this.outputs?[(int)IoPorts.ElevatorMotor] ?? false;

        public bool MeasureBarrierOn => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public bool ResetSecurity => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        #endregion

        #region Methods

        public bool UpdateInputStates(bool[] newInputStates)
        {
            if (this.inputs.Length != newInputStates.Length)
            {
                throw new IOException($"Input states length mismatch while updating I/O driver status");
            }

            bool updateRequired = false;
            for (int index = 0; index < TOTAL_INPUTS; index++)
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

            return updateRequired;
        }

        public bool UpdateOutputStates(bool[] newOutputStates)
        {
            bool updateRequired = false;

            if (this.outputs.Length != newOutputStates.Length)
            {
                throw new IOException($"Output states length mismatch while updating I/O driver status");
            }

            for (int index = 0; index < TOTAL_OUTPUTS; index++)
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
