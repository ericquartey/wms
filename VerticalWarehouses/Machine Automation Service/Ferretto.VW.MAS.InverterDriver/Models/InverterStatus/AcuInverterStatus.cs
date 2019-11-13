﻿using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus
{
    public class AcuInverterStatus : InverterStatusBase, IAcuInverterStatus, IPositioningInverterStatus, IHomingInverterStatus
    {
        #region Fields

        private const int TOTAL_SENSOR_INPUTS = 8;

        private int currentPosition;

        #endregion

        #region Constructors

        public AcuInverterStatus(InverterIndex systemIndex)
            : base(systemIndex)
        {
            this.Inputs = new bool[TOTAL_SENSOR_INPUTS];
        }

        #endregion

        //INFO ACU Inputs

        #region Properties

        public bool ACU_EncoderChannelA => this.Inputs?[(int)InverterSensors.ACU_EncoderChannelA] ?? false;

        public bool ACU_EncoderChannelB => this.Inputs?[(int)InverterSensors.ACU_EncoderChannelB] ?? false;

        public bool ACU_EncoderChannelZ => this.Inputs?[(int)InverterSensors.ACU_EncoderChannelZ] ?? false;

        public bool ACU_FreeSensor1 => this.Inputs?[(int)InverterSensors.ACU_FreeSensor1] ?? false;

        public bool ACU_FreeSensor2 => this.Inputs?[(int)InverterSensors.ACU_FreeSensor2] ?? false;

        public bool ACU_HardwareSensorSS1 => this.Inputs?[(int)InverterSensors.ACU_HardwareSensorSS1] ?? false;

        public bool ACU_HardwareSensorSTO => this.Inputs?[(int)InverterSensors.ACU_HardwareSensorSTO] ?? false;

        public bool ACU_ZeroSensor => this.Inputs?[(int)InverterSensors.ACU_ZeroSensor] ?? false;
        //public bool ACU_HardwareSensorSTOB => this.Inputs?[(int)InverterSensors.ACU_HardwareSensorSTOB] ?? false;

        public int CurrentPosition => this.currentPosition;


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

                throw new InvalidCastException($"Current Control Word Type {this.controlWord.GetType().Name} is not compatible with Homing Mode");
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

                throw new InvalidCastException($"Current Status Word Type {this.statusWord.GetType().Name} is not compatible with Homing Mode");
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

                throw new InvalidCastException($"Current Control Word Type {this.controlWord.GetType().Name} is not compatible with Position Mode");
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

                throw new InvalidCastException($"Current Status Word Type {this.statusWord.GetType().Name} is not compatible with Position Mode");
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

                throw new InvalidCastException($"Current Control Word Type {this.controlWord.GetType().Name} is not compatible with TableTravel Mode");
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

                throw new InvalidCastException($"Current Status Word Type {this.statusWord.GetType().Name} is not compatible with TableTravel Mode");
            }
        }

        #endregion

        #region Methods

        public override bool UpdateInputsStates(bool[] newInputStates)
        {
            if (newInputStates is null)
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

                if (this.Inputs[index] != newInputStates[index])
                {
                    updateRequired = true;
                    break;
                }
            }
            try
            {
                if (updateRequired)
                {
                    Array.Copy(newInputStates, 0, this.Inputs, 0, newInputStates.Length);
                }
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Error while updating ACU inverter inputs.", ex);
            }

            return updateRequired;
        }

        public bool UpdateInverterCurrentPosition(Axis axisToMove, int position)
        {
            var updateRequired = false;

            if (this.currentPosition != position)
            {
                this.currentPosition = position;
                updateRequired = true;
            }

            return updateRequired;
        }

        #endregion
    }
}
