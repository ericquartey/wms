using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(CellPosition))]
    public class CellPosition : BaseModel<int>
    {
        #region Properties

        public string Description { get; set; }

        #endregion
    }
}
