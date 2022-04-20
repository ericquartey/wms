using System.ComponentModel.DataAnnotations.Schema;

namespace Ferretto.VW.MAS.NordDriver
{
    public interface IStatusWord
    {
        #region Properties

        [Column(Order = 3)]
        bool IsFault { get; }

        [Column(Order = 1)]
        bool IsOperationEnabled { get; }

        [Column(Order = 2)]
        bool IsQuickStopTrue { get; }

        [Column(Order = 0)]
        bool IsReadyToSwitchOn { get; }

        [Column(Order = 4)]
        bool IsTargetReached { get; }

        [Column(Order = 7)]
        bool IsWarning { get; }

        [Column(Order = 5)]
        bool ParameterSet1 { get; }

        [Column(Order = 6)]
        bool ParameterSet2 { get; }

        ushort Value { get; set; }

        #endregion
    }
}
