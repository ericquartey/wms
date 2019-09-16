﻿using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.InverterStatus
{
    public class AngInverterStatus : InverterStatusBase, IAngInverterStatus
    {
        #region Fields

        public bool[] angInverterInputs;

        private const int TOTAL_SENSOR_INPUTS = 8;

        private int currentPositionAxisHorizontal;

        private int currentPositionAxisVertical;

        private bool waitingHeartbeatAck;

        #endregion

        #region Constructors

        public AngInverterStatus(byte systemIndex)
        {
            this.SystemIndex = systemIndex;
            this.angInverterInputs = new bool[TOTAL_SENSOR_INPUTS];
            this.InverterType = InverterType.Ang;
        }

        #endregion

        //INFO ANG Inputs

        //public bool ANG_BarrierCalibration => this.angInverterInputs?[(int)InverterSensors.ANG_BarrierCalibration] ?? false;

        //public bool ANG_ElevatorMotorTemprature => this.angInverterInputs?[(int)InverterSensors.ANG_ElevatorMotorTemprature] ?? false;

        #region Properties

        public bool ANG_EncoderChannelACradle => this.angInverterInputs?[(int)InverterSensors.ANG_EncoderChannelACradle] ?? false;

        public bool ANG_EncoderChannelBCradle => this.angInverterInputs?[(int)InverterSensors.ANG_EncoderChannelBCradle] ?? false;

        public bool ANG_EncoderChannelZCradle => this.angInverterInputs?[(int)InverterSensors.ANG_EncoderChannelZCradle] ?? false;

        public bool ANG_HardwareSensorSS1 => this.angInverterInputs?[(int)InverterSensors.ANG_HardwareSensorSS1] ?? false;

        public bool ANG_HardwareSensorSTO => this.angInverterInputs?[(int)InverterSensors.ANG_HardwareSensorSTO] ?? false;

        public bool ANG_OverrunElevatorSensor => this.angInverterInputs?[(int)InverterSensors.ANG_OverrunElevatorSensor] ?? false;

        public bool ANG_ZeroCradleSensor => this.angInverterInputs?[(int)InverterSensors.ANG_ZeroCradleSensor] ?? false;

        public bool ANG_ZeroElevatorSensor => this.angInverterInputs?[(int)InverterSensors.ANG_ZeroElevatorSensor] ?? false;
        //public bool ANG_HardwareSensorSTOB => this.angInverterInputs?[(int)InverterSensors.ANG_HardwareSensorSTOB] ?? false;

        //public bool ANG_OpticalMeasuringBarrier => this.angInverterInputs?[(int)InverterSensors.ANG_OpticalMeasuringBarrier] ?? false;

        //public bool ANG_PresenceDrawerOnCradleMachineSide => this.angInverterInputs?[(int)InverterSensors.ANG_PresenceDrawerOnCradleMachineSide] ?? false;

        //public bool ANG_PresenceDraweronCradleOperatoreSide => this.angInverterInputs?[(int)InverterSensors.ANG_PresenceDraweronCradleOperatoreSide] ?? false;
        public int CurrentPositionAxisHorizontal => this.currentPositionAxisHorizontal;

        public int CurrentPositionAxisVertical => this.currentPositionAxisVertical;

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

        public bool[] Inputs => this.angInverterInputs;

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

        public ITableTravelControlWord TableTravelControlWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.TableTravel)
                {
                    throw new InvalidOperationException("Inverter is not configured for TableTravel Mode");
                }

                if (this.controlWord is ITableTravelControlWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Control Word Type {this.controlWord.GetType()} is not compatible with TableTravel Mode");
            }
        }

        public ITableTravelStatusWord TableTravelStatusWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.TableTravel)
                {
                    throw new InvalidOperationException("Inverter is not configured for TableTravel Mode");
                }

                if (this.statusWord is ITableTravelStatusWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Status Word Type {this.statusWord.GetType()} is not compatible with TableTravel Mode");
            }
        }

        public bool WaitingHeartbeatAck
        {
            get
            {
                lock (this)
                {
                    return this.waitingHeartbeatAck;
                }
            }
            set
            {
                lock (this)
                {
                    this.waitingHeartbeatAck = value;
                }
            }
        }

        #endregion

        #region Methods

        public bool UpdateANGInverterCurrentPosition(Axis axisToMove, int position)
        {
            var updateRequired = false;

            switch (axisToMove)
            {
                case Axis.Both:
                case Axis.Vertical:
                    if (this.currentPositionAxisVertical != position)
                    {
                        this.currentPositionAxisVertical = position;
                        updateRequired = true;
                    }
                    break;

                case Axis.Horizontal:
                    if (this.currentPositionAxisHorizontal != position)
                    {
                        this.currentPositionAxisHorizontal = position;
                        updateRequired = true;
                    }
                    break;

                case Axis.None:
                    break;
            }

            return updateRequired;
        }

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
                    break;
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
