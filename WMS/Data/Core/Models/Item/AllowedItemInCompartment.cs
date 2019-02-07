namespace Ferretto.WMS.Data.Core.Models
{
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

        public int? MaxCapacity { get; set; }

        #endregion
    }
}
