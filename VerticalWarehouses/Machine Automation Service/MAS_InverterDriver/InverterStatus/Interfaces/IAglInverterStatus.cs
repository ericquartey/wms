using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces
{
    public interface IAglInverterStatus : IInverterStatusBase
    {
        #region Properties

        bool AGL_FreeSensor1 { get; }

        bool AGL_FreeSensor2 { get; }

        bool AGL_FreeSensor3 { get; }

        bool AGL_FreeSensor4 { get; }

        bool AGL_HardwareSensorSS1 { get; }

        bool AGL_HardwareSensorSTOA { get; }

        bool AGL_HardwareSensorSTOB { get; }

        bool AGL_ShutterSensorA { get; }

        bool AGL_ShutterSensorB { get; }

        ShutterPosition CurrentShutterPosition { get; }

        IProfileVelocityControlWord ProfileVelocityControlWord { get; }

        IProfileVelocityStatusWord ProfileVelocityStatusWord { get; }

        ShutterType ShutterType { get; }

        #endregion
    }
}
