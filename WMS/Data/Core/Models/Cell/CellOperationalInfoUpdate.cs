using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Cell))]
    public class CellOperationalInfoUpdate : BaseModel<int>
    {
        #region Properties

        public int Priority { get; set; }

        public string Status { get; set; }

        #endregion
    }
}
