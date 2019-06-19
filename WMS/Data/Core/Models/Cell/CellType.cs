using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(CellType))]
    public class CellType : BaseModel<int>
    {
        #region Properties

        public string Description { get; set; }

        #endregion
    }
}
