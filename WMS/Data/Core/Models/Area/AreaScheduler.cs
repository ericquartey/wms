using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Models
{
    public class AreaScheduler : Model<int>
    {
        #region Properties

        public IEnumerable<BayScheduler> Bays { get; set; }

        #endregion
    }
}
