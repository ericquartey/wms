using System.ComponentModel.DataAnnotations.Schema;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IAcuInverterStatus : IInverterStatusBase
    {
        #region Properties

        [Column(Order = 4)]
        bool ACU_EncoderChannelA { get; }

        [Column(Order = 3)]
        bool ACU_EncoderChannelB { get; }

        [Column(Order = 5)]
        bool ACU_EncoderChannelZ { get; }

        [Column(Order = 6)]
        bool ACU_FreeSensor1 { get; }

        [Column(Order = 7)]
        bool ACU_FreeSensor2 { get; }

        [Column(Order = 1)]
        bool ACU_HardwareSensorSS1 { get; }

        [Column(Order = 0)]
        bool ACU_HardwareSensorSTO { get; }

        [Column(Order = 2)]
        bool ACU_ZeroSensor { get; }

        #endregion
    }
}
