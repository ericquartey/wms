using System.ComponentModel.DataAnnotations.Schema;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IAglInverterStatus : IInverterStatusBase
    {
        #region Properties

        [Column(Order = (int)InverterSensors.AGL_FreeSensor1)]
        bool AGL_FreeSensor1 { get; }

        [Column(Order = (int)InverterSensors.AGL_FreeSensor2)]
        bool AGL_FreeSensor2 { get; }

        [Column(Order = (int)InverterSensors.AGL_FreeSensor3)]
        bool AGL_FreeSensor3 { get; }

        [Column(Order = (int)InverterSensors.AGL_FreeSensor4)]
        bool AGL_FreeSensor4 { get; }

        [Column(Order = (int)InverterSensors.AGL_HardwareSensorSS1)]
        bool AGL_HardwareSensorSS1 { get; }

        [Column(Order = (int)InverterSensors.AGL_HardwareSensorSTO)]
        bool AGL_HardwareSensorSTO { get; }

        [Column(Order = (int)InverterSensors.AGL_ShutterSensorA)]
        bool AGL_ShutterSensorA { get; }

        [Column(Order = (int)InverterSensors.AGL_ShutterSensorB)]
        bool AGL_ShutterSensorB { get; }

        ShutterPosition CurrentShutterPosition { get; }

        IProfileVelocityControlWord ProfileVelocityControlWord { get; }

        IProfileVelocityStatusWord ProfileVelocityStatusWord { get; }

        #endregion
    }
}
