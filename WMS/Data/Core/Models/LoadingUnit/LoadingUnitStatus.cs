using System.Collections.Generic;
using System.Linq;

namespace Ferretto.WMS.Data.Core.Models
{
    public class LoadingUnitStatus : BaseModel<string>
    {
        #region Properties

        public string Description { get; set; }

        #endregion
    }
}
