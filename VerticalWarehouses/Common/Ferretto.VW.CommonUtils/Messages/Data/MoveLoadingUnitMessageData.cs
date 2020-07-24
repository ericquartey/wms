using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    //    [Serializable]
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
            this.LoadUnitId = other.LoadUnitId;
            this.InsertLoadUnit = other.InsertLoadUnit;
            this.MissionId = other.MissionId;
            this.CommandAction = other.CommandAction;
            this.StopReason = other.StopReason;
            this.MissionStep = other.MissionStep;
            this.Verbosity = other.Verbosity;
        }

        #endregion

        #region Properties

        public CommandAction CommandAction { get; }

        public LoadingUnitLocation Destination { get; }

        public int? DestinationCellId { get; set; }

        public bool InsertLoadUnit { get; }

        public int? LoadUnitId { get; }

        public int? MissionId { get; }

        public MissionStep MissionStep { get; }

        public MissionType MissionType { get; set; }

        public LoadingUnitLocation Source { get; }

        public int? SourceCellId { get; }

        public StopRequestReason StopReason { get; }

        public MessageVerbosity Verbosity { get; }

        public int? WmsId { get; set; }

        #endregion
    }
}
