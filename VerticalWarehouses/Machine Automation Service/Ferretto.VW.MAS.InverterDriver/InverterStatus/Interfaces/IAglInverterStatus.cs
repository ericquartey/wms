using System.ComponentModel.DataAnnotations.Schema;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IAglInverterStatus : IInverterStatusBase
    {
        #region Properties

        [Column(Order = 4)]
        bool AGL_FreeSensor1 { get; }

        [Column(Order = 5)]
        bool AGL_FreeSensor2 { get; }

        [Column(Order = 6)]
        bool AGL_FreeSensor3 { get; }

        [Column(Order = 7)]
        bool AGL_FreeSensor4 { get; }

        [Column(Order = 1)]
        bool AGL_HardwareSensorSS1 { get; }

        [Column(Order = 0)]
        bool AGL_HardwareSensorSTO { get; }

        [Column(Order = 2)]
        bool AGL_ShutterSensorA { get; }

        [Column(Order = 3)]
        bool AGL_ShutterSensorB { get; }

        ShutterPosition CurrentShutterPosition { get; }

        IProfileVelocityControlWord ProfileVelocityControlWord { get; }

        IProfileVelocityStatusWord ProfileVelocityStatusWord { get; }

        ShutterType ShutterType { get; }

        #endregion
    }
}
