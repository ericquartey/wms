using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Aisle))]
    public class Aisle : BaseModel<int>
    {
        #region Properties

        public int AreaId { get; set; }

        public string AreaName { get; set; }

        public string Name { get; set; }

        #endregion
    }
}
