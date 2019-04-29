namespace Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus
{
    public interface IStatusWord
    {
        #region Properties

        bool IsFault { get; }

        bool IsOperationEnabled { get; }

        bool IsQuickStopActive { get; }

        bool IsReadyToSwitchOn { get; }

        bool IsRemote { get; }

        bool IsSwitchedOn { get; }

        bool IsSwitchOnDisabled { get; }

        bool IsVoltageEnabled { get; }

        bool IsWarning { get; }

        bool IsWarning2 { get; }

        ushort Value { get; set; }

        #endregion
    }
}
