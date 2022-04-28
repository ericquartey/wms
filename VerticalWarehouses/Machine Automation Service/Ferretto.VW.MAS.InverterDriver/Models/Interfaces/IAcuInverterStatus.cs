using System.ComponentModel.DataAnnotations.Schema;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IAcuInverterStatus : IInverterStatusBase
    {
        #region Properties

        [Column(Order = (int)InverterSensors.ACU_EncoderChannelA)]
        bool ACU_EncoderChannelA { get; }

        [Column(Order = (int)InverterSensors.ACU_EncoderChannelB)]
        bool ACU_EncoderChannelB { get; }

        [Column(Order = (int)InverterSensors.ACU_FreeSensor1)]
        bool ACU_FreeSensor1 { get; }

        [Column(Order = (int)InverterSensors.ACU_FreeSensor2)]
        bool ACU_FreeSensor2 { get; }

        [Column(Order = (int)InverterSensors.ACU_HardwareSensorSS1)]
        bool ACU_HardwareSensorSS1 { get; }

        [Column(Order = (int)InverterSensors.ACU_HardwareSensorSTO)]
        bool ACU_HardwareSensorSTO { get; }

        [Column(Order = (int)InverterSensors.ACU_ZeroSensor)]
        bool ACU_ZeroSensor { get; }

        [Column(Order = (int)InverterSensors.ACU_ZeroSensorTop)]
        bool ACU_ZeroSensorTop { get; }

        #endregion
    }
}
