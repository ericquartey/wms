using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Ferretto.VW.MAS.IODriver
{
    internal class IoReadMessage
    {
        #region Fields

        private readonly byte[] data;

        private readonly ShdFormatDataOperation formatDataOperation;

        private readonly byte fwRelease;

        private readonly bool[] inputs;

        private readonly bool[] outputs;

        #endregion

        #region Constructors

        public IoReadMessage(
            ShdFormatDataOperation formatDataOperation,
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
        }

        #endregion

        #region Properties

        public bool BayLightOn => this.outputs?[(int)IoPorts.BayLight] ?? false;

        public bool CradleMotorOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public bool ElevatorMotorOn => this.outputs?[(int)IoPorts.ElevatorMotor] ?? false;

        public bool EndMissionRobotOn => this.outputs?[(int)IoPorts.EndMissionRobot] ?? false;

        public ShdFormatDataOperation FormatDataOperation => this.formatDataOperation;

        public byte FwRelease => this.fwRelease;

        public bool[] Inputs => this.inputs;

        public bool MeasureProfileOn => this.outputs?[(int)IoPorts.MeasureProfile] ?? false;

        public bool[] Outputs => this.outputs;

        public bool OutputsCleared => !this.outputs?.Any(o => o) ?? false;

        public bool PowerEnable => this.outputs?[(int)IoPorts.PowerEnable] ?? false;

        public bool ReadyWarehouseRobotOn => this.outputs?[(int)IoPorts.ReadyWarehouseRobot] ?? false;

        public bool ResetSecurity => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

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
