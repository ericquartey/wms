using System.Collections.Generic;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Area))]
    public class Area : BaseModel<int>
    {
        #region Properties

        public string Name { get; set; }

        public IEnumerable<Bay> Bays { get; set; }

        #endregion
    }
}
