using System.ComponentModel.DataAnnotations.Schema;

namespace Ferretto.VW.MAS.InverterDriver.Contracts
{
    public interface IStatusWord
    {
        #region Properties

        [Column(Order = 3)]
        bool IsFault { get; }

        bool IsOperationEnabled { get; }

        bool IsQuickStopTrue { get; }

        [Column(Order = 0)]
        bool IsReadyToSwitchOn { get; }

        [Column(Order = 9)]
        bool IsRemote { get; }

        [Column(Order = 1)]
        bool IsSwitchedOn { get; }

        [Column(Order = 6)]
        bool IsSwitchOnDisabled { get; }

        [Column(Order = 4)]
        bool IsVoltageEnabled { get; }

        [Column(Order = 7)]
        bool IsWarning { get; }

        [Column(Order = 15)]
        bool IsWarning2 { get; }

        ushort Value { get; set; }

        #endregion
    }
}
