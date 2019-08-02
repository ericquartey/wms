using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IAglInverterStatus : IInverterStatusBase
    {
        #region Properties

        bool AGL_FreeSensor1 { get; }

        bool AGL_FreeSensor2 { get; }

        bool AGL_FreeSensor3 { get; }

        bool AGL_FreeSensor4 { get; }

        bool AGL_HardwareSensorSS1 { get; }

        bool AGL_HardwareSensorSTO { get; }

        bool AGL_ShutterSensorA { get; }

        bool AGL_ShutterSensorB { get; }

        ShutterPosition CurrentShutterPosition { get; }

        IProfileVelocityControlWord ProfileVelocityControlWord { get; }

        IProfileVelocityStatusWord ProfileVelocityStatusWord { get; }

        ShutterType ShutterType { get; }

        #endregion
    }
}
