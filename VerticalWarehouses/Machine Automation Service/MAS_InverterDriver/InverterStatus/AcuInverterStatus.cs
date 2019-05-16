using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Exceptions;

namespace Ferretto.VW.MAS_InverterDriver.InverterStatus
{
    public class AcuInverterStatus : InverterStatusBase, IAcuInverterStatus
    {
        #region Fields

        private const int TOTAL_SENSOR_INPUTS = 8;

        public bool[] acuInverterInputs;

        #endregion

        #region Constructors

        public AcuInverterStatus()
        {
            this.acuInverterInputs = new bool[TOTAL_SENSOR_INPUTS];
        }

        #endregion

        #region Properties

        //INFO ACU Inputs

        public bool ACU_HardwareSensorSTOA => this.acuInverterInputs?[(int)InverterSensors.ACU_HardwareSensorSTOA] ?? false;

        public bool ACU_HardwareSensorSS1 => this.acuInverterInputs?[(int)InverterSensors.ACU_HardwareSensorSS1] ?? false;

        public bool ACU_ZeroSensor => this.acuInverterInputs?[(int)InverterSensors.ACU_ZeroSensor] ?? false;

        public bool ACU_EncoderCanalB => this.acuInverterInputs?[(int)InverterSensors.ACU_EncoderCanalB] ?? false;

        public bool ACU_EncoderCanalA => this.acuInverterInputs?[(int)InverterSensors.ACU_EncoderCanalA] ?? false;

        public bool ACU_FreeSensor1 => this.acuInverterInputs?[(int)InverterSensors.ACU_FreeSensor1] ?? false;

        public bool ACU_HardwareSensorSTOB => this.acuInverterInputs?[(int)InverterSensors.ACU_HardwareSensorSTOB] ?? false;

        public bool ACU_FreeSensor2 => this.acuInverterInputs?[(int)InverterSensors.ACU_FreeSensor2] ?? false;

        #endregion

        #region Methods

        public bool UpdateACUInverterInputsStates(bool[] newInputStates)
        {
            if (newInputStates == null)
            {
                return false;
            }

            var updateRequired = false;
            for (var index = 0; index < newInputStates.Length; index++)
            {
                if (index > TOTAL_SENSOR_INPUTS)
                {
                    continue;
                }

                if (this.acuInverterInputs[index] != newInputStates[index])
                {
                    updateRequired = true;
                    break;
                }
            }
            try
            {
                if (updateRequired)
                {
                    Array.Copy(newInputStates, 0, this.acuInverterInputs, 0, newInputStates.Length);
                }
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while updating ACU Inputs status");
            }

            return updateRequired;
        }

        #endregion
    }
}
