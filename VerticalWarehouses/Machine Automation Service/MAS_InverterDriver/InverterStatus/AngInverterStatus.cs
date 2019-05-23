using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Exceptions;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.InverterStatus
{
    public class AngInverterStatus : InverterStatusBase, IAngInverterStatus
    {

        #region Fields

        private const int TOTAL_SENSOR_INPUTS = 11;

        public bool[] angInverterInputs;

        #endregion

        #region Constructors

        public AngInverterStatus(byte systemIndex)
        {
            this.SystemIndex = systemIndex;
            this.angInverterInputs = new bool[TOTAL_SENSOR_INPUTS];
        }

        #endregion

        #region Properties

        //INFO ANG Inputs

        public bool ANG_HardwareSensorSTOA => this.angInverterInputs?[(int)InverterSensors.ANG_HardwareSensorSTOA] ?? false;

        public bool ANG_HardwareSensorSS1 => this.angInverterInputs?[(int)InverterSensors.ANG_HardwareSensorSS1] ?? false;

        public bool ANG_ZeroElevatorSensor => this.angInverterInputs?[(int)InverterSensors.ANG_ZeroElevatorSensor] ?? false;

        public bool ANG_OverrunElevatorSensor => this.angInverterInputs?[(int)InverterSensors.ANG_OverrunElevatorSensor] ?? false;

        public bool ANG_BarrierCalibration => this.angInverterInputs?[(int)InverterSensors.ANG_BarrierCalibration] ?? false;

        public bool ANG_ZeroCradleSensor => this.angInverterInputs?[(int)InverterSensors.ANG_ZeroCradleSensor] ?? false;

        public bool ANG_HardwareSensorSTOB => this.angInverterInputs?[(int)InverterSensors.ANG_HardwareSensorSTOB] ?? false;

        public bool ANG_OpticalMeasuringBarrier => this.angInverterInputs?[(int)InverterSensors.ANG_OpticalMeasuringBarrier] ?? false;

        public bool ANG_PresenceDrawerOnCradleMachineSide => this.angInverterInputs?[(int)InverterSensors.ANG_PresenceDrawerOnCradleMachineSide] ?? false;

        public bool ANG_PresenceDraweronCradleOperatoreSide => this.angInverterInputs?[(int)InverterSensors.ANG_PresenceDraweronCradleOperatoreSide] ?? false;

        public bool ANG_ElevatorMotorTemprature => this.angInverterInputs?[(int)InverterSensors.ANG_ElevatorMotorTemprature] ?? false;

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

        public IPositionControlWord PositionControlWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.Position)
                {
                    throw new InvalidOperationException("Inverter is not configured for Position Mode");
                }

                if (this.controlWord is IPositionControlWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Control Word Type {this.controlWord.GetType()} is not compatible with Position Mode");
            }
        }

        public IPositionStatusWord PositionStatusWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.Position)
                {
                    throw new InvalidOperationException("Inverter is not configured for Position Mode");
                }

                if (this.statusWord is IPositionStatusWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Status Word Type {this.statusWord.GetType()} is not compatible with Position Mode");
            }
        }

        #endregion

        #region Methods

        public bool UpdateANGInverterInputsStates(bool[] newInputStates)
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

                if (this.angInverterInputs[index] != newInputStates[index])
                {
                    updateRequired = true;
                    break;
                }
            }
            try
            {
                if (updateRequired)
                {
                    Array.Copy(newInputStates, 0, this.angInverterInputs, 0, newInputStates.Length);
                }
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while updating ANG Inputs status");
            }

            return updateRequired;
        }

        #endregion
    }
}
