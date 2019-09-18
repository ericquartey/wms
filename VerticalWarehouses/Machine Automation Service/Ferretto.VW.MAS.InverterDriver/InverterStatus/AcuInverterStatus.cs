using System;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;

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

        public bool ACU_EncoderChannelA => this.acuInverterInputs?[(int)InverterSensors.ACU_EncoderChannelA] ?? false;

        public bool ACU_EncoderChannelB => this.acuInverterInputs?[(int)InverterSensors.ACU_EncoderChannelB] ?? false;

        public bool ACU_EncoderChannelZ => this.acuInverterInputs?[(int)InverterSensors.ACU_EncoderChannelZ] ?? false;

        public bool ACU_FreeSensor1 => this.acuInverterInputs?[(int)InverterSensors.ACU_FreeSensor1] ?? false;

        public bool ACU_FreeSensor2 => this.acuInverterInputs?[(int)InverterSensors.ACU_FreeSensor2] ?? false;

        public bool ACU_HardwareSensorSS1 => this.acuInverterInputs?[(int)InverterSensors.ACU_HardwareSensorSS1] ?? false;

        public bool ACU_HardwareSensorSTO => this.acuInverterInputs?[(int)InverterSensors.ACU_HardwareSensorSTO] ?? false;

        public bool ACU_ZeroSensor => this.acuInverterInputs?[(int)InverterSensors.ACU_ZeroSensor] ?? false;
        //public bool ACU_HardwareSensorSTOB => this.acuInverterInputs?[(int)InverterSensors.ACU_HardwareSensorSTOB] ?? false;

        public IHomingControlWord HomingControlWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.Homing)
                {
                    throw new InvalidOperationException("Inverter is not configured for Homing Mode");
                }

                if (this.controlWord is IHomingControlWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Control Word Type {this.controlWord.GetType()} is not compatible with Homing Mode");
            }
        }

        public IHomingStatusWord HomingStatusWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.Homing)
                {
                    throw new InvalidOperationException("Inverter is not configured for Homing Mode");
                }

                if (this.statusWord is IHomingStatusWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Status Word Type {this.statusWord.GetType()} is not compatible with Homing Mode");
            }
        }

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
