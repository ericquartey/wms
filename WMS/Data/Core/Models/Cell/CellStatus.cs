using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(CellStatus))]
    public class CellStatus : BaseModel<int>
    {
        #region Properties

        public string Description { get; set; }

        #endregion
    }
}
