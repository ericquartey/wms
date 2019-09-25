using System.ComponentModel.DataAnnotations.Schema;

namespace Ferretto.VW.CommonUtils.Enumerations
{
    public interface IStatusWord
    {
        #region Properties

        [Column(Order = 3)]
        bool IsFault { get; }

        [Column(Order = 2)]
        bool IsOperationEnabled { get; }

        [Column(Order = 5)]
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
