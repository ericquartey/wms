using System;
using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    public sealed class Mission : IDataModel, ITimestamped
    {
        #region Properties

        public Bay Bay { get; set; }

        public int? BayId { get; set; }

        public DateTime CreationDate { get; set; }

        public int Id { get; set; }

        public DateTime LastModificationDate { get; set; }

        public LoadingUnit LoadingUnit { get; set; }

        public int LoadingUnitId { get; set; }

        public IEnumerable<MissionOperation> Operations { get; set; }

        public int Priority { get; set; }

        #endregion
    }
}
