using System.ComponentModel.DataAnnotations.Schema;

namespace Ferretto.VW.CommonUtils.Enumerations
{
    public interface IStatusWord
    {
        #region Properties

        [Column(Order = 4)]
        bool IsFault { get; }

        [Column(Order = 3)]
        bool IsOperationEnabled { get; }

        [Column(Order = 6)]
        bool IsQuickStopTrue { get; }

        [Column(Order = 1)]
        bool IsReadyToSwitchOn { get; }

        [Column(Order = 10)]
        bool IsRemote { get; }

        [Column(Order = 2)]
        bool IsSwitchedOn { get; }

        [Column(Order = 7)]
        bool IsSwitchOnDisabled { get; }

        [Column(Order = 5)]
        bool IsVoltageEnabled { get; }

        [Column(Order = 8)]
        bool IsWarning { get; }

        [Column(Order = 16)]
        bool IsWarning2 { get; }

        [Column(Order = 1)]
        ushort Value { get; set; }

        #endregion
    }
}
