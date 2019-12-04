using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class Mission : DataModel
    {
        #region Properties

        public DateTime CreationDate { get; set; }

        public int? DestinationCellId { get; set; }

        public Guid FsmId { get; set; }

        public string FsmRestoreStateName { get; set; }

        public string FsmStateName { get; set; }

        public int? LoadingUnitCellSourceId { get; set; }

        public LoadingUnitLocation LoadingUnitDestination { get; set; }

        public int LoadingUnitId { get; set; }

        public LoadingUnitLocation LoadingUnitSource { get; set; }

        public MissionType MissionType { get; set; }

        public int Priority { get; set; }

        public bool RestoreConditions { get; set; }

        public MissionStatus Status { get; set; }

        public BayNumber TargetBay { get; set; }

        public int? WmsId { get; set; }

        #endregion

        #region Methods

        public bool IsMissionToRestore()
        {
            return !string.IsNullOrEmpty(this.FsmRestoreStateName);
        }

        public bool IsRestoringType()
        {
            return this.MissionType == MissionType.WMS
                || this.MissionType == MissionType.IN
                || this.MissionType == MissionType.Manual   // TODO only for testing! please remove this line
                || this.MissionType == MissionType.OUT;
        }

        #endregion
    }
}
