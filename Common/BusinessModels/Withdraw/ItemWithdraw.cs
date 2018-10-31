using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public sealed class ItemWithdraw : BusinessObject
    {
        #region Fields

        private IEnumerable<Area> areaChoices;
        private int areaId;
        private IEnumerable<Bay> bayChoices;
        private int bayId;
        private ItemDetails item;
        private int quantity;

        #endregion Fields

        #region Properties

        public IEnumerable<Area> AreaChoices
        {
            get => this.areaChoices;
            set => this.SetProperty(ref this.areaChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawArea), ResourceType = typeof(BusinessObjects))]
        public int AreaId
        {
            get => this.areaId;
            set => this.SetProperty(ref this.areaId, value);
        }

        public IEnumerable<Bay> BayChoices
        {
            get => this.bayChoices;
            set => this.SetProperty(ref this.bayChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawBay), ResourceType = typeof(BusinessObjects))]
        public int BayId
        {
            get => this.bayId;
            set => this.SetProperty(ref this.bayId, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawItem), ResourceType = typeof(BusinessObjects))]
        public ItemDetails Item
        {
            get => this.item;
            set
            {
                this.SetProperty(ref this.item, value);
                this.ItemCode = value.Code;
                this.ItemDescription = value.Description;
                this.MeasureUnitDescription = value.MeasureUnitDescription;
                this.ItemManagementTypeDescription = value.ItemManagementTypeDescription;
                this.TotalAvailable = value.TotalAvailable;
            }
        }

        [Display(Name = nameof(BusinessObjects.ItemCode), ResourceType = typeof(BusinessObjects))]
        public string ItemCode { get; private set; }

        [Display(Name = nameof(BusinessObjects.ItemDescription), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get; private set; }

        [Display(Name = nameof(BusinessObjects.ItemManagementType), ResourceType = typeof(BusinessObjects))]
        public string ItemManagementTypeDescription { get; private set; }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawLot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get; set; }

        [Display(Name = nameof(BusinessObjects.MeasureUnitDescription), ResourceType = typeof(BusinessObjects))]
        public string MeasureUnitDescription { get; private set; }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawQuantity), ResourceType = typeof(BusinessObjects))]
        public int Quantity
        {
            get => this.quantity;
            set => this.SetProperty(ref this.quantity, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawRegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawSub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawSub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemAvailable), ResourceType = typeof(BusinessObjects))]
        public int TotalAvailable { get; private set; }

        #endregion Properties
    }
}
