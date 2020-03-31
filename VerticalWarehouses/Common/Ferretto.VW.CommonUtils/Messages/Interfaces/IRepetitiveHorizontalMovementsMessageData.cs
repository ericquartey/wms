using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IRepetitiveHorizontalMovementsMessageData : IMessageData
    {
        #region Properties

        BayNumber BayNumber { get; set; }

        int BayPositionId { get; set; }

        bool BypassConditions { get; set; }

        int Delay { get; set; }

        int ExecutedCycles { get; set; }

        bool IsTestStopped { get; set; }

        int? LoadingUnitId { get; }

        int RequiredCycles { get; set; }

        int? SourceBayPositionId { get; set; }

        int? TargetBayPositionId { get; set; }

        #endregion
    }
}
