using System;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.InverterStatus
{
    public class InverterIoStatus
    {
        #region Fields

        private const int TOTAL_INPUTS = 8;

        private readonly bool[] inputs = new bool[TOTAL_INPUTS];

        #endregion

        #region Properties

        public bool EmergencyStop => this.inputs?[(int)InverterIoPorts.EmergencyStop] ?? false;

        public bool[] Inputs => this.inputs;

        public bool PowerOn => this.inputs?[(int)InverterIoPorts.Power] ?? false;

        #endregion

        #region Methods

        public bool UpdateInputStates(bool[] newInputStates)
        {
            if (this.inputs.Length != newInputStates.Length)
            {
                throw new InverterDriverException($"Input states length mismatch while updating I/O driver status");
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
                throw new InverterDriverException($"Error while updating inverter inputs.", ex);
            }

            return updateRequired;
        }

        public bool UpdateInputStates(ushort newInputStates)
        {
            if (this.inputs.Length != TOTAL_INPUTS)
            {
                throw new InverterDriverException($"Input states length mismatch while updating I/O driver status");
            }

            var updateRequired = false;

            for (var index = 0; index < TOTAL_INPUTS; index++)
            {
                var newValue = (newInputStates & 0x0001 << index) > 0;

                if (this.inputs[index] != newValue)
                {
                    this.inputs[index] = newValue;
                    updateRequired = true;
                }
            }

            return updateRequired;
        }

        #endregion
    }
}
