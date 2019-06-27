using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Item))]
    public class AllowedItemInCompartment : BaseModel<int>
    {
        #region Properties

        public string AbcClassDescription { get; set; }

        public string AbcClassId { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public string ItemCategoryDescription { get; set; }

        public int? ItemCategoryId { get; set; }

        [Positive]
        public double? MaxCapacity { get; set; }

        #endregion
    }
}
