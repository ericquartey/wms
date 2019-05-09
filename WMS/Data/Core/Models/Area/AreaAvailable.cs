using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Models
{
    public class AreaAvailable : Model<int>
    {
        #region Properties

        public IEnumerable<BayAvailable> Bays { get; set; }

        #endregion
    }
}
