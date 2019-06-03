using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class AllowedItemArea : BaseModel<int>, IItemAreaDeletePolicy
    {
        #region Properties

        public bool IsItemInArea { get; set; }

        public string Name { get; set; }

        public double TotalStock { get; set; }

        #endregion
    }
}
