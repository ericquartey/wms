using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(ItemArea))]
    public class ItemArea : BaseModel<int>
    {
        #region Properties

        public int AreaId { get; set; }

        public int ItemId { get; set; }

        #endregion
    }
}
