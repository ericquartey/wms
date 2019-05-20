using System;
using System.IO;
using System.Linq;
using System.Text;
using Ferretto.VW.MAS_IODriver.Enumerations;

namespace Ferretto.VW.MAS_IODriver
{
    public class IoSHDReadMessage
    {
        #region Fields

        private const byte RELEASE_FW_10 = 0x10;

        private const byte RELEASE_FW_11 = 0x11;

        private const int TOTAL_DATA_BYTES = 17;

        private const int TOTAL_INPUTS = 16;

        private const int TOTAL_OUTPUTS = 8;

        private readonly byte[] data;

        private readonly byte errorCode;

        private readonly SHDFormatDataOperation formatDataOperation;

        private readonly byte fwRelease;

        private readonly bool[] inputs;

        private readonly bool[] outputs;

        #endregion

        #region Constructors

        public IoSHDReadMessage(
            SHDFormatDataOperation formatDataOperation,
            byte fwRelease,
            bool[] inputs,
            bool[] outputs,
            byte[] data,
            byte errorCode)
        {
            this.formatDataOperation = formatDataOperation;
            this.fwRelease = fwRelease;

            if (inputs != null)
            {
                this.inputs = new bool[inputs.Length];

                try
                {
                    Array.Copy(inputs, this.inputs, inputs.Length);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Exception {ex.Message} while initializing Inputs status");
                }
            }
            else
            {
                this.inputs = null;
            }

            if (outputs != null)
            {
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
            else
            {
                this.outputs = null;
            }

            if (data != null)
            {
                this.data = new byte[data.Length];
                try
                {
                    Array.Copy(data, this.data, data.Length);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Exception {ex.Message} while initializing Data payload");
                }
            }
            else
            {
                this.data = null;
            }
        }

        #endregion

        #region Properties

        public bool BayLightOn => this.outputs?[(int)IoPorts.BayLight] ?? false;

        public bool CradleMotorOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public bool ElevatorMotorOn => this.outputs?[(int)IoPorts.ElevatorMotor] ?? false;

        public SHDFormatDataOperation FormatDataOperation => this.formatDataOperation;

        public byte FwRelease => this.fwRelease;

        public bool[] Inputs => this.inputs;

        public bool MeasureBarrierOn => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public bool[] Outputs => this.outputs;

        public bool OutputsCleared => !this.outputs?.Any(o => o) ?? false;

        public bool ResetSecurity => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public bool ValidData => this.data != null;

        public bool ValidInputs => this.inputs != null;

        public bool ValidOutputs => this.outputs != null;

        #endregion

        #region Methods

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

        #endregion
    }
}
