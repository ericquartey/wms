namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IAcuInverterStatus : IInverterStatusBase
    {
        #region Properties

        bool ACU_EncoderChannelA { get; }

        bool ACU_EncoderChannelB { get; }

        bool ACU_EncoderChannelZ { get; }

        bool ACU_FreeSensor1 { get; }

        bool ACU_FreeSensor2 { get; }

        bool ACU_HardwareSensorSS1 { get; }

        bool ACU_HardwareSensorSTO { get; }

        bool ACU_ZeroSensor { get; }

        #endregion
    }
}
