using System.ComponentModel.DataAnnotations.Schema;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IAngInverterStatus : IInverterStatusBase
    {
        #region Properties

        [Column(Order = 4)]
        bool ANG_EncoderChannelACradle { get; }

        [Column(Order = 3)]
        bool ANG_EncoderChannelBCradle { get; }

        [Column(Order = 5)]
        bool ANG_EncoderChannelZCradle { get; }

        [Column(Order = 1)]
        bool ANG_HardwareSensorSS1 { get; }

        [Column(Order = 0)]
        bool ANG_HardwareSensorSTO { get; }

        [Column(Order = 6)]
        bool ANG_OverrunElevatorSensor { get; }

        [Column(Order = 7)]
        bool ANG_ZeroCradleSensor { get; }

        [Column(Order = 2)]
        bool ANG_ZeroElevatorSensor { get; }

        //bool ANG_BarrierCalibration { get; }

        //bool ANG_ElevatorMotorTemprature { get; }

        //bool ANG_HardwareSensorSTOA { get; }

        //bool ANG_HardwareSensorSTOB { get; }

        //bool ANG_OpticalMeasuringBarrier { get; }

        //bool ANG_PresenceDrawerOnCradleMachineSide { get; }

        //bool ANG_PresenceDraweronCradleOperatoreSide { get; }

        IHomingControlWord HomingControlWord { get; }

        IHomingStatusWord HomingStatusWord { get; }

        IPositionControlWord PositionControlWord { get; }

        IPositionStatusWord PositionStatusWord { get; }

        ITableTravelControlWord TableTravelControlWord { get; }

        ITableTravelStatusWord TableTravelStatusWord { get; }

        #endregion
    }
}
