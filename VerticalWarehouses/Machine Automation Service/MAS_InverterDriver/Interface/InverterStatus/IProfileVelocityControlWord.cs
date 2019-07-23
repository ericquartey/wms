namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface IProfileVelocityControlWord : IControlWord
    {
        #region Properties

        bool EnableOperation { set; }

        bool EnableVoltage { set; }

        bool FaultReset { set; }

        bool Halt { set; }

        bool QuickStop { set; }

        bool SwitchOn { set; }

        #endregion
    }
}
