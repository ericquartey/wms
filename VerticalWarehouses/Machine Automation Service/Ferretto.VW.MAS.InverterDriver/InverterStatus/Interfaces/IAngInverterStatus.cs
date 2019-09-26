﻿using System.ComponentModel.DataAnnotations.Schema;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IAngInverterStatus : IInverterStatusBase
    {
        #region Properties

        [Column(Order = (int)InverterSensors.ANG_EncoderChannelACradle)]
        bool ANG_EncoderChannelACradle { get; }

        [Column(Order = (int)InverterSensors.ANG_EncoderChannelBCradle)]
        bool ANG_EncoderChannelBCradle { get; }

        [Column(Order = (int)InverterSensors.ANG_EncoderChannelZCradle)]
        bool ANG_EncoderChannelZCradle { get; }

        [Column(Order = (int)InverterSensors.ANG_HardwareSensorSS1)]
        bool ANG_HardwareSensorSS1 { get; }

        [Column(Order = (int)InverterSensors.ANG_HardwareSensorSTO)]
        bool ANG_HardwareSensorSTO { get; }

        [Column(Order = (int)InverterSensors.ANG_OverrunElevatorSensor)]
        bool ANG_OverrunElevatorSensor { get; }

        [Column(Order = (int)InverterSensors.ANG_ZeroCradleSensor)]
        bool ANG_ZeroCradleSensor { get; }

        [Column(Order = (int)InverterSensors.ANG_ZeroElevatorSensor)]
        bool ANG_ZeroElevatorSensor { get; }

        #endregion

        //bool ANG_BarrierCalibration { get; }

        //bool ANG_ElevatorMotorTemprature { get; }

        //bool ANG_HardwareSensorSTOA { get; }

        //bool ANG_HardwareSensorSTOB { get; }

        //bool ANG_OpticalMeasuringBarrier { get; }

        //bool ANG_PresenceDrawerOnCradleMachineSide { get; }

        //bool ANG_PresenceDraweronCradleOperatoreSide { get; }
    }
}
