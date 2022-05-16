using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus
{
    public class AngInverterStatus : InverterStatusBase, IAngInverterStatus, IPositioningInverterStatus, IHomingInverterStatus
    {
        #region Fields

        private const int TOTAL_SENSOR_INPUTS = 8;

        private int currentPositionAxisHorizontal;

        private int currentPositionAxisVertical;

        #endregion

        #region Constructors

        public AngInverterStatus(InverterIndex systemIndex, int? canOpenNode)
            : base(systemIndex, canOpenNode)
        {
            this.Inputs = new bool[TOTAL_SENSOR_INPUTS];
        }

        #endregion

        //INFO ANG Inputs

        //public bool ANG_BarrierCalibration => this.Inputs?[(int)InverterSensors.ANG_BarrierCalibration] ?? false;

        //public bool ANG_ElevatorMotorTemprature => this.Inputs?[(int)InverterSensors.ANG_ElevatorMotorTemprature] ?? false;

        #region Properties

        public bool ANG_ElevatorOverrunSensor => this.Inputs?[(int)InverterSensors.ANG_ElevatorOverrunSensor] ?? false;

        public bool ANG_EncoderChannelACradle => this.Inputs?[(int)InverterSensors.ANG_EncoderChannelACradle] ?? false;

        public bool ANG_EncoderChannelBCradle => this.Inputs?[(int)InverterSensors.ANG_EncoderChannelBCradle] ?? false;

        public bool ANG_HardwareSensorSS1 => this.Inputs?[(int)InverterSensors.ANG_HardwareSensorSS1] ?? false;

        public bool ANG_HardwareSensorSTO => this.Inputs?[(int)InverterSensors.ANG_HardwareSensorSTO] ?? false;

        public bool ANG_ZeroCradleSensor => this.Inputs?[(int)InverterSensors.ANG_ZeroCradleSensor] ?? false;

        public bool ANG_ZeroElevatorSensor => this.Inputs?[(int)InverterSensors.ANG_ZeroElevatorSensor] ?? false;
        //public bool ANG_HardwareSensorSTOB => this.Inputs?[(int)InverterSensors.ANG_HardwareSensorSTOB] ?? false;

        //public bool ANG_OpticalMeasuringBarrier => this.Inputs?[(int)InverterSensors.ANG_OpticalMeasuringBarrier] ?? false;

        //public bool ANG_PresenceDrawerOnCradleMachineSide => this.Inputs?[(int)InverterSensors.ANG_PresenceDrawerOnCradleMachineSide] ?? false;

        //public bool ANG_PresenceDraweronCradleOperatoreSide => this.Inputs?[(int)InverterSensors.ANG_PresenceDraweronCradleOperatoreSide] ?? false;
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

                throw new InvalidOperationException($"Current Control Word Type {this.controlWord.GetType().Name} is not compatible with Homing Mode");
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

                throw new InvalidOperationException($"Current Status Word Type {this.statusWord.GetType().Name} is not compatible with Homing Mode");
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

                throw new InvalidOperationException($"Current Control Word Type {this.controlWord.GetType().Name} is not compatible with Position Mode");
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

                throw new InvalidOperationException($"Current Status Word Type {this.statusWord.GetType().Name} is not compatible with Position Mode");
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

                throw new InvalidOperationException($"Current Control Word Type {this.controlWord.GetType().Name} is not compatible with TableTravel Mode");
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

                throw new InvalidOperationException($"Current Status Word Type {this.statusWord.GetType().Name} is not compatible with TableTravel Mode");
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
                    break;
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
                throw new InverterDriverException("Error while updating ANG inverter inputs.", ex);
            }

            return updateRequired;
        }

        public bool UpdateInverterCurrentPosition(Axis axisToMove, int position)
        {
            var updateRequired = false;

            switch (axisToMove)
            {
                case Axis.HorizontalAndVertical:
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

        #endregion
    }
}
