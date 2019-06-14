using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(ItemArea))]
    public class AllowedItemArea : BaseModel<int>, IItemAreaDeletePolicy
    {
        #region Properties

        public bool IsItemInArea { get; set; }

        public string Name { get; set; }

        [PositiveOrZero]
        public double TotalStock { get; set; }

        #endregion
    }
}
