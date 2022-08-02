using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class MoveLoadingUnitMessageData : IMoveLoadingUnitMessageData
    {
        #region Constructors

        public MoveLoadingUnitMessageData()
        {
        }

        public MoveLoadingUnitMessageData(
            MissionType missionType,
            LoadingUnitLocation source,
            LoadingUnitLocation destination,
            int? sourceCellId,
            int? destinationCellId,
            int? loadUnitId,
            bool insertLoadUnit = false,
            int? missionId = null,
            int? loadUnitHeight = null,
            int? netWeight = null,
            CommandAction commandAction = CommandAction.Start,
            StopRequestReason stopReason = StopRequestReason.NoReason,
            MissionStep step = MissionStep.NotDefined,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.MissionType = missionType;
            this.Source = source;
            this.Destination = destination;
            this.SourceCellId = sourceCellId;
            this.DestinationCellId = destinationCellId;
            this.LoadUnitId = loadUnitId;
            this.InsertLoadUnit = insertLoadUnit;
            this.MissionId = missionId;
            this.LoadUnitHeight = loadUnitHeight;
            this.NetWeight = netWeight;
            this.CommandAction = commandAction;
            this.StopReason = stopReason;
            this.MissionStep = step;
            this.Verbosity = verbosity;
        }

        public MoveLoadingUnitMessageData(IMoveLoadingUnitMessageData other)
        {
            this.MissionType = other.MissionType;
            this.Source = other.Source;
            this.Destination = other.Destination;
            this.SourceCellId = other.SourceCellId;
            this.DestinationCellId = other.DestinationCellId;
            this.LoadUnitHeight = other.LoadUnitHeight;
            this.LoadUnitId = other.LoadUnitId;
            this.InsertLoadUnit = other.InsertLoadUnit;
            this.MissionId = other.MissionId;
            this.NetWeight = other.NetWeight;
            this.CommandAction = other.CommandAction;
            this.StopReason = other.StopReason;
            this.MissionStep = other.MissionStep;
            this.Verbosity = other.Verbosity;
        }

        #endregion

        #region Properties

        public CommandAction CommandAction { get; set; }

        public DateTimeOffset CreationDate { get; set; } = DateTime.Now;        // TODO: why not use UtcNow???

        public LoadingUnitLocation Destination { get; set; }

        public int? DestinationCellId { get; set; }

        public bool InsertLoadUnit { get; set; }

        public int? LoadUnitHeight { get; set; }

        public int? LoadUnitId { get; set; }

        public int? MissionId { get; set; }

        public MissionStep MissionStep { get; set; }

        public MissionType MissionType { get; set; }

        public int? NetWeight { get; set; }

        public LoadingUnitLocation Source { get; set; }

        public int? SourceCellId { get; set; }

        public StopRequestReason StopReason { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        public int? WmsId { get; set; }

        #endregion
    }
}
