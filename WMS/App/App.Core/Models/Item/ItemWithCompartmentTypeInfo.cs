using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class ItemWithCompartmentTypeInfo : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassDescription { get; set; }

        public string AbcClassId { get; set; }

        [Display(Name = nameof(BusinessObjects.Code), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        [Display(Name = nameof(BusinessObjects.Description), ResourceType = typeof(BusinessObjects))]
        public string Description { get; set; }

        public string Image { get; set; }

        [Display(Name = nameof(BusinessObjects.Category), ResourceType = typeof(BusinessObjects))]
        public string ItemCategoryDescription { get; set; }

        public int? ItemCategoryId { get; set; }

        [Display(Name = nameof(BusinessObjects.MaxCapacity), ResourceType = typeof(BusinessObjects))]
        public double? MaxCapacity { get; set; }

        [Display(Name = nameof(BusinessObjects.MeasureUnitDescription), ResourceType = typeof(BusinessObjects))]
        public string MeasureUnitDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemAvailable), ResourceType = typeof(BusinessObjects))]
        public double TotalAvailable { get; set; }

        [Display(Name = nameof(BusinessObjects.ReservedForPick), ResourceType = typeof(BusinessObjects))]
        public double TotalReservedForPick { get; set; }

        [Display(Name = nameof(BusinessObjects.ReservedToPut), ResourceType = typeof(BusinessObjects))]
        public double TotalReservedToPut { get; set; }

        [Display(Name = nameof(BusinessObjects.Stock), ResourceType = typeof(BusinessObjects))]
        public double TotalStock { get; set; }

        #endregion
    }
}
