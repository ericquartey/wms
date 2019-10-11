using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class MoveLoadingUnitMessageData : IMoveLoadingUnitMessageData
    {
        #region Constructors

        public MoveLoadingUnitMessageData(
            LoadingUnitDestination source,
            LoadingUnitDestination destination,
            int? sourceCellId,
            int? destinationCellId,
            int? loadingUnitId,
            CommandAction commandAction = CommandAction.Start,
            StopRequestReason stopReason = StopRequestReason.NoReason,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Source = source;
            this.Destination = destination;
            this.SourceCellId = sourceCellId;
            this.DestinationCellId = destinationCellId;
            this.LoadingUnitId = loadingUnitId;
            this.CommandAction = commandAction;
            this.StopReason = stopReason;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public CommandAction CommandAction { get; }

        public LoadingUnitDestination Destination { get; }

        public int? DestinationCellId { get; }

        public int? LoadingUnitId { get; }

        public LoadingUnitDestination Source { get; }

        public int? SourceCellId { get; }

        public StopRequestReason StopReason { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
