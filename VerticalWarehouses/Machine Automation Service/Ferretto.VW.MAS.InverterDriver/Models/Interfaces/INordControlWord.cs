using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface INordControlWord : IControlWord
    {
        #region Properties

        bool ControlWordValid { set; }

        bool DisableVoltage { set; }

        bool EnableAcceleration { set; }

        bool EnableRamp { set; }

        bool EnableSetPoint { set; }

        bool NotReadyForOperation { set; }

        bool ParameterSet1 { set; }

        bool ParameterSet2 { set; }

        bool RotationLeft { set; }

        bool RotationRight { set; }

        bool Start480_11 { set; }

        bool Start480_12 { set; }

        #endregion
    }
}
