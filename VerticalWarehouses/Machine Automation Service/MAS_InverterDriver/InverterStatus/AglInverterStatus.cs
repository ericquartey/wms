using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Exceptions;

namespace Ferretto.VW.MAS_InverterDriver.InverterStatus
{
    public class AglInverterStatus : InverterStatusBase, IAglInverterStatus
    {
        #region Fields

        private const int TOTAL_SENSOR_INPUTS = 9;

        public bool[] aglInverterInputs;

        #endregion

        #region Constructors

        public AglInverterStatus()
        {
            this.aglInverterInputs = new bool[TOTAL_SENSOR_INPUTS];
        }

        #endregion

        #region Properties

        //INFO AGL Inputs

        public bool AGL_HardwareSensorSTOA => this.aglInverterInputs?[(int)InverterSensors.AGL_HardwareSensorSTOA] ?? false;

        public bool AGL_HardwareSensorSS1 => this.aglInverterInputs?[(int)InverterSensors.AGL_HardwareSensorSS1] ?? false;

        public bool AGL_ShutterSensorA => this.aglInverterInputs?[(int)InverterSensors.AGL_ShutterSensorA] ?? false;

        public bool AGL_ShutterSensorB => this.aglInverterInputs?[(int)InverterSensors.AGL_ShutterSensorB] ?? false;

        public bool AGL_FreeSensor1 => this.aglInverterInputs?[(int)InverterSensors.AGL_FreeSensor1] ?? false;

        public bool AGL_FreeSensor2 => this.aglInverterInputs?[(int)InverterSensors.AGL_FreeSensor2] ?? false;

        public bool AGL_FreeSensor3 => this.aglInverterInputs?[(int)InverterSensors.AGL_FreeSensor3] ?? false;

        public bool AGL_FreeSensor4 => this.aglInverterInputs?[(int)InverterSensors.AGL_FreeSensor4] ?? false;

        public bool AGL_HardwareSensorSTOB => this.aglInverterInputs?[(int)InverterSensors.AGL_HardwareSensorSTOB] ?? false;

        #endregion

        #region Methods

        public bool UpdateAGLInverterInputsStates(bool[] newInputStates)
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

                if (this.aglInverterInputs[index] != newInputStates[index])
                {
                    updateRequired = true;
                    break;
                }
            }
            try
            {
                if (updateRequired)
                {
                    Array.Copy(newInputStates, 0, this.aglInverterInputs, 0, newInputStates.Length);
                }
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while updating AGL Inputs status");
            }

            return updateRequired;
        }

        #endregion
    }
}
