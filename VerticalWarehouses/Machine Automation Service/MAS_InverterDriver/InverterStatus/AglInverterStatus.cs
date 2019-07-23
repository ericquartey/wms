using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Exceptions;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus
{
    public class AglInverterStatus : InverterStatusBase, IAglInverterStatus
    {
        #region Fields

        public bool[] aglInverterInputs;

        private const int TOTAL_SENSOR_INPUTS = 9;

        private readonly ShutterType shutterType;

        private ShutterPosition currentShutterPosition;

        #endregion

        #region Constructors

        public AglInverterStatus(byte systemIndex)
        {
            this.SystemIndex = systemIndex;
            this.aglInverterInputs = new bool[TOTAL_SENSOR_INPUTS];
            this.currentShutterPosition = ShutterPosition.Opened; // Set the Opened position (workaround)
            this.OperatingMode = (ushort)InverterOperationMode.ProfileVelocity;
            this.InverterType = MAS_Utils.Enumerations.InverterType.Agl;
            this.shutterType = ShutterType.Shutter2Type;
        }

        #endregion

        //INFO AGL Inputs

        #region Properties

        public bool AGL_FreeSensor1 => this.aglInverterInputs?[(int)InverterSensors.AGL_FreeSensor1] ?? false;

        public bool AGL_FreeSensor2 => this.aglInverterInputs?[(int)InverterSensors.AGL_FreeSensor2] ?? false;

        public bool AGL_FreeSensor3 => this.aglInverterInputs?[(int)InverterSensors.AGL_FreeSensor3] ?? false;

        public bool AGL_FreeSensor4 => this.aglInverterInputs?[(int)InverterSensors.AGL_FreeSensor4] ?? false;

        public bool AGL_HardwareSensorSS1 => this.aglInverterInputs?[(int)InverterSensors.AGL_HardwareSensorSS1] ?? false;

        public bool AGL_HardwareSensorSTOA => this.aglInverterInputs?[(int)InverterSensors.AGL_HardwareSensorSTOA] ?? false;

        public bool AGL_HardwareSensorSTOB => this.aglInverterInputs?[(int)InverterSensors.AGL_HardwareSensorSTOB] ?? false;

        public bool AGL_ShutterSensorA => this.aglInverterInputs?[(int)InverterSensors.AGL_ShutterSensorA] ?? false;

        public bool AGL_ShutterSensorB => this.aglInverterInputs?[(int)InverterSensors.AGL_ShutterSensorB] ?? false;

        public ShutterPosition CurrentShutterPosition
        {
            get => this.GetCurrentPosition();
            private set => this.currentShutterPosition = value;
        }

        public bool[] Inputs => this.aglInverterInputs;

        public IProfileVelocityControlWord ProfileVelocityControlWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.ProfileVelocity)
                {
                    throw new InvalidOperationException("Inverter is not configured for ProfileVelocity Mode");
                }

                if (this.controlWord is IProfileVelocityControlWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Control Word Type {this.controlWord.GetType()} is not compatible with ProfileVelocity Mode");
            }
        }

        public IProfileVelocityStatusWord ProfileVelocityStatusWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.ProfileVelocity)
                {
                    throw new InvalidOperationException("Inverter is not configured for ProfileVelocity Mode");
                }

                if (this.statusWord is IProfileVelocityStatusWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Status Word Type {this.statusWord.GetType()} is not compatible with ProfileVelocity Mode");
            }
        }

        public ShutterType ShutterType => this.shutterType;

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
                    this.GetCurrentPosition();
                }
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while updating AGL Inputs status");
            }

            return updateRequired;
        }

        private ShutterPosition GetCurrentPosition()
        {
            var value = ShutterPosition.None;
            if (this.aglInverterInputs[(int)InverterSensors.AGL_ShutterSensorA])
            {
                if (this.aglInverterInputs[(int)InverterSensors.AGL_ShutterSensorB])
                {
                    value = ShutterPosition.Closed;
                }
                else
                {
                    value = ShutterPosition.Undefined;
                }
            }
            else
            {
                if (this.aglInverterInputs[(int)InverterSensors.AGL_ShutterSensorB])
                {
                    value = ShutterPosition.Half;
                }
                else
                {
                    value = ShutterPosition.Opened;
                }
            }

            this.currentShutterPosition = value;
            return value;
        }

        #endregion
    }
}
