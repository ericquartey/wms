using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public class Mission : DataModel
    {
        #region Properties

        public DateTime CreationDate { get; set; }

        public bool IsWmsMission => this.WmsId.HasValue;

        public MissionStatus MissionStatus { get; set; }

        public int? WmsId { get; set; }

        #endregion
    }
}
