using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(ItemCategory))]
    public sealed class ItemCategory : BaseModel<int>
    {
        #region Properties

        public string Description { get; set; }

        #endregion
    }
}
