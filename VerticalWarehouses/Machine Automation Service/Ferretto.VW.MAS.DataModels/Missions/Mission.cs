using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class Mission : DataModel
    {
        #region Properties

        public BayNumber BayNumber { get; set; }

        public DateTime CreationDate { get; set; }

        public int LoadingUnitId { get; set; }

        public int Priority { get; set; }

        public MissionStatus Status { get; set; }

        public int? WmsId { get; set; }

        public int WmsPriority { get; set; }

        #endregion
    }
}
