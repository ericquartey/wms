using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IAngInverterStatus : IInverterStatusBase
    {
        #region Properties

        bool ANG_BarrierCalibration { get; }

        bool ANG_ElevatorMotorTemprature { get; }

        bool ANG_HardwareSensorSS1 { get; }

        bool ANG_HardwareSensorSTOA { get; }

        bool ANG_HardwareSensorSTOB { get; }

        bool ANG_OpticalMeasuringBarrier { get; }

        bool ANG_OverrunElevatorSensor { get; }

        bool ANG_PresenceDrawerOnCradleMachineSide { get; }

        bool ANG_PresenceDraweronCradleOperatoreSide { get; }

        bool ANG_ZeroCradleSensor { get; }

        bool ANG_ZeroElevatorSensor { get; }

        IHomingControlWord HomingControlWord { get; }

        IHomingStatusWord HomingStatusWord { get; }

        IPositionControlWord PositionControlWord { get; }

        IPositionStatusWord PositionStatusWord { get; }

        #endregion
    }
}
