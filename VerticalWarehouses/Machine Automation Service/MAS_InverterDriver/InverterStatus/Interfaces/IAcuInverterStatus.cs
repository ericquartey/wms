namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IAcuInverterStatus : IInverterStatusBase
    {
        #region Properties

        bool ACU_EncoderCanalA { get; }

        bool ACU_EncoderCanalB { get; }

        bool ACU_FreeSensor1 { get; }

        bool ACU_FreeSensor2 { get; }

        bool ACU_HardwareSensorSS1 { get; }

        bool ACU_HardwareSensorSTOA { get; }

        bool ACU_HardwareSensorSTOB { get; }

        bool ACU_ZeroSensor { get; }

        #endregion
    }
}
