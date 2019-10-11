using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMoveLoadingUnitMessageData : IMessageData
    {
        #region Properties

        CommandAction CommandAction { get; }

        LoadingUnitDestination Destination { get; }

        int? DestinationCellId { get; }

        int? LoadingUnitId { get; }

        LoadingUnitDestination Source { get; }

        int? SourceCellId { get; }

        StopRequestReason StopReason { get; }

        #endregion
    }
}
