using System;
using System.IO;
using System.Linq;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_IODriver.Enumerations;

namespace Ferretto.VW.MAS_IODriver
{
    public class IoMessage
    {
        #region Fields

        private bool[] inputs;

        private bool[] outputs;

        #endregion

        #region Constructors

        public IoMessage()
        {
            this.Force = false;
        }

        public IoMessage(bool read)
        {
            this.Force = false;

            if (read)
            {
                this.inputs = new bool[8];
            }
            else
            {
                this.outputs = new bool[5];
            }
        }

        public IoMessage(int inputs, int outputs)
        {
            this.Force = false;

            if (inputs > 0)
            {
                this.inputs = new bool[inputs];
            }

            if (outputs > 0)
            {
                this.outputs = new bool[outputs];
            }
        }

        public IoMessage(bool[] data, bool input)
        {
            this.Force = false;

            if (input)
            {
                this.inputs = new bool[data.Length];
                try
                {
                    Array.Copy(data, this.inputs, data.Length);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Exception {ex.Message} while initializing Inputs status");
                }
            }
            else
            {
                this.outputs = new bool[data.Length];
                try
                {
                    Array.Copy(data, this.outputs, data.Length);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Exception {ex.Message} while initializing Outputs status");
                }
            }
        }

        public IoMessage(bool[] inputs, bool[] outputs)
        {
            this.Force = false;

            this.inputs = new bool[inputs.Length];
            this.outputs = new bool[outputs.Length];

            try
            {
                Array.Copy(inputs, this.inputs, inputs.Length);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while initializing Inputs status");
            }

            try
            {
                Array.Copy(outputs, this.outputs, outputs.Length);
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while initializing Outputs status");
            }
        }

        #endregion

        #region Properties

        public bool BayLightOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public bool CradleMotorOn => this.outputs?[(int)IoPorts.CradleMotor] ?? false;

        public bool ElevatorMotorOn => this.outputs?[(int)IoPorts.ElevatorMotor] ?? false;

        public bool Force { get; set; }

        public bool[] Inputs => this.inputs;

        public bool MeasureBarrierOn => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public bool[] Outputs => this.outputs;

        public bool OutputsCleared => !this.outputs?.Any(o => o) ?? false;

        public bool ResetSecurity => this.outputs?[(int)IoPorts.ResetSecurity] ?? false;

        public bool ValidInputs => this.inputs != null;

        public bool ValidOutputs => this.outputs != null;

        #endregion

        #region Methods

        public bool SwitchCradleMotor(bool switchOn)
        {
            if (this.outputs == null)
            {
                throw new ArgumentNullException(nameof(Outputs), "Message Digital Outputs are not initialized correctly");
            }

            if (switchOn)
            {
                if (this.outputs[(int)IoPorts.ElevatorMotor])
                {
                    return false;
                }

                this.outputs[(int)IoPorts.CradleMotor] = true;
            }
            else
            {
                this.outputs[(int)IoPorts.CradleMotor] = false;
            }

            return true;
        }

        public bool SwitchElevatorMotor(bool switchOn)
        {
            if (this.outputs == null)
            {
                throw new ArgumentNullException(nameof(Outputs), "Message Digital Outputs are not initialized correctly");
            }

            if (switchOn)
            {
                if (this.outputs[(int)IoPorts.CradleMotor])
                {
                    return false;
                }

                this.outputs[(int)IoPorts.ElevatorMotor] = true;
            }
            else
            {
                this.outputs[(int)IoPorts.ElevatorMotor] = false;
            }

            return true;
        }

        public bool SwitchResetSecurity(bool switchOn)
        {
            if (this.outputs == null)
            {
                throw new ArgumentNullException(nameof(Outputs), "Message Digital Outputs are not initialized correctly");
            }

            this.outputs[(int)IoPorts.ResetSecurity] = switchOn;

            return true;
        }

        #endregion
    }
}
