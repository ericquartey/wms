using System;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus
{
    public class AcuInverterStatus : InverterStatusBase, IAcuInverterStatus
    {
        #region Fields

        public bool[] acuInverterInputs;

        private const int TOTAL_SENSOR_INPUTS = 8;

        #endregion

        #region Constructors

        public AcuInverterStatus(byte systemIndex)
        {
            this.SystemIndex = systemIndex;
            this.acuInverterInputs = new bool[TOTAL_SENSOR_INPUTS];
            this.InverterType = InverterType.Acu;
        }

        #endregion

        //INFO ACU Inputs

        #region Properties

        public bool ACU_EncoderCanalA => this.acuInverterInputs?[(int)InverterSensors.ACU_EncoderCanalA] ?? false;

        public bool ACU_EncoderCanalB => this.acuInverterInputs?[(int)InverterSensors.ACU_EncoderCanalB] ?? false;

        public bool ACU_FreeSensor1 => this.acuInverterInputs?[(int)InverterSensors.ACU_FreeSensor1] ?? false;

        public bool ACU_FreeSensor2 => this.acuInverterInputs?[(int)InverterSensors.ACU_FreeSensor2] ?? false;

        public bool ACU_HardwareSensorSS1 => this.acuInverterInputs?[(int)InverterSensors.ACU_HardwareSensorSS1] ?? false;

        public bool ACU_HardwareSensorSTOA => this.acuInverterInputs?[(int)InverterSensors.ACU_HardwareSensorSTOA] ?? false;

        public bool ACU_HardwareSensorSTOB => this.acuInverterInputs?[(int)InverterSensors.ACU_HardwareSensorSTOB] ?? false;

        public bool ACU_ZeroSensor => this.acuInverterInputs?[(int)InverterSensors.ACU_ZeroSensor] ?? false;

        public bool[] Inputs => this.acuInverterInputs;

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
