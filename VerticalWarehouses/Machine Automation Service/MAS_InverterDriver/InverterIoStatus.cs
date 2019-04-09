using System;
using System.Collections.Specialized;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_Utils.Exceptions;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver
{
    public class InverterIoStatus
    {
        #region Fields

        private const int TOTAL_INPUTS = 16;

        private readonly bool[] inputs;

        #endregion

        #region Constructors

        public InverterIoStatus()
        {
            this.inputs = new bool[TOTAL_INPUTS];
        }

        #endregion

        #region Properties

        public bool EmergencyStop => this.inputs?[(int)InverterIoPorts.EmergencyStop] ?? false;

        public bool[] Inputs => this.inputs;

        public bool PowerOn => this.inputs?[(int)InverterIoPorts.Power] ?? false;

        #endregion

        #region Methods

        public bool UpdateInputStates(bool[] newInputStates)
        {
            if (this.inputs?.Length != newInputStates.Length)
            {
                throw new InverterDriverException($"Input states length mismatch while updating I/O driver status");
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
                throw new InverterDriverException($"Exception {ex.Message} while updating Inputs status");
            }

            return updateRequired;
        }

        public bool UpdateInputStates(ushort newInputStates)
        {
            if (this.inputs.Length != 16)
            {
                throw new InverterDriverException($"Input states length mismatch while updating I/O driver status");
            }
            bool updateRequired = false;

            var inputBits = new BitVector32(newInputStates);

            for (int index = 0; index < TOTAL_INPUTS; index++)
            {
                if (this.inputs[index] != inputBits[index])
                {
                    this.inputs[index] = inputBits[index];
                    updateRequired = true;
                }
            }

            return updateRequired;
        }

        #endregion
    }
}
