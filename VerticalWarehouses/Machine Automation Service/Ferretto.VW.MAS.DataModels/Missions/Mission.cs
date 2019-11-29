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

        public string FsmStateName { get; set; }

        public int? LoadingUnitCellSourceId { get; set; }

        public int LoadingUnitId { get; set; }

        public LoadingUnitLocation LoadingUnitSource { get; set; }

        public MissionType MissionType { get; set; }

        public int Priority { get; set; }

        public MissionStatus Status { get; set; }

        public BayNumber TargetBay { get; set; }

        public int? WmsId { get; set; }

        #endregion
    }
}
