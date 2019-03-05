using System;
using System.IO;

namespace Ferretto.VW.MAS_IODriver
{
    public class IoStatus
    {
        #region Fields

        private const int totalInputs = 8;

        private const int totalOutputs = 5;

        private bool[] inputs;

        private bool[] outputs;

        #endregion

        #region Constructors

        public IoStatus()
        {
            this.inputs = new bool[totalInputs];
            this.outputs = new bool[totalOutputs];
        }

        public IoStatus(IoStatus status)
        {
            this.inputs = new bool[totalInputs];
            this.outputs = new bool[totalOutputs];
        }

        #endregion

        #region Methods

        public bool UpdateInputStates(bool[] newInputStates)
        {
            bool updateRequired = false;
            for (int index = 0; index < totalInputs; index++)
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
                    Array.Copy(newInputStates, this.inputs, totalInputs);
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
            for (int index = 0; index < totalOutputs; index++)
            {
                if (this.outputs[index] != newOutputStates[index])
                {
                    updateRequired = true;
                    break;
                }
            }
            try
            {
                if (updateRequired)
                {
                    Array.Copy(newOutputStates, this.outputs, totalOutputs);
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while updating Outputs status");
            }

            return updateRequired;
        }

        #endregion
    }
}
