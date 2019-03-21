using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class Compartment : BusinessObject
    {
        #region Fields

        private string itemMeasureUnit;

        private int stock;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.CompartmentStatus), ResourceType = typeof(BusinessObjects))]
        public string CompartmentStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentType), ResourceType = typeof(BusinessObjects))]
        public string CompartmentTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentIsItemPairingFixed), ResourceType = typeof(BusinessObjects))]
        public bool IsItemPairingFixed { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemDescription_extended), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get; set; }

        public string ItemMeasureUnit
        {
            get => this.itemMeasureUnit;
            set => this.SetProperty(ref this.itemMeasureUnit, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCode_extended), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitCode { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentLot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get; set; }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public string MaterialStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentStock), ResourceType = typeof(BusinessObjects))]
        public int Stock
        {
            get => this.stock;
            set => this.SetProperty(ref this.stock, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentSub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentSub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get; set; }

        #endregion
    }
}
