using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class MoveLoadingUnitMessageData : IMoveLoadingUnitMessageData
    {
        #region Constructors

        public MoveLoadingUnitMessageData(
            MissionType missionType,
            LoadingUnitLocation source,
            LoadingUnitLocation destination,
            int? sourceCellId,
            int? destinationCellId,
            int? loadingUnitId,
            bool insertLoadingUnit = false,
            bool ejectLoadingUnit = false,
            Guid? missionId = null,
            CommandAction commandAction = CommandAction.Start,
            StopRequestReason stopReason = StopRequestReason.NoReason,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.MissionType = missionType;
            this.Source = source;
            this.Destination = destination;
            this.SourceCellId = sourceCellId;
            this.DestinationCellId = destinationCellId;
            this.LoadingUnitId = loadingUnitId;
            this.InsertLoadingUnit = insertLoadingUnit;
            this.EjectLoadingUnit = ejectLoadingUnit;
            this.MissionId = missionId;
            this.CommandAction = commandAction;
            this.StopReason = stopReason;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public CommandAction CommandAction { get; }

        public LoadingUnitLocation Destination { get; }

        public int? DestinationCellId { get; }

        public bool EjectLoadingUnit { get; }

        public bool InsertLoadingUnit { get; }

        public int? LoadingUnitId { get; }

        public Guid? MissionId { get; }

        public MissionType MissionType { get; set; }

        public LoadingUnitLocation Source { get; }

        public int? SourceCellId { get; }

        public StopRequestReason StopReason { get; }

        public BayNumber TargetBay { get; set; }

        public MessageVerbosity Verbosity { get; }

        public int? WmsId { get; set; }

        #endregion
    }
}
