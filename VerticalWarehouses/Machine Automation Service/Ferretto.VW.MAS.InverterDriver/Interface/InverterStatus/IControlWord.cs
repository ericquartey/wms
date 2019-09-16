namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface IControlWord
    {
        #region Properties

        bool EnableOperation { set; }

        bool EnableVoltage { set; }

        bool FaultReset { get; set; }

        bool Halt { set; }

        bool HeartBeat { get; set; }

        bool HorizontalAxis { get; set; }

        bool QuickStop { set; }

        bool SwitchOn { set; }

        ushort Value { get; set; }

        #endregion
    }
}
