using System.Collections.Generic;

namespace Ferretto.WMS.Data.Core.Models
{
    public class AreaAvailable : BaseModel<int>
    {
        #region Properties

        public IEnumerable<BayAvailable> Bays { get; set; }

        #endregion
    }
}
