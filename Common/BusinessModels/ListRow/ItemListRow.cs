using System;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class ItemListRow : BusinessObject
    {
        #region Fields

        private string code;
        private DateTime creationDate;
        private int dispatchedQuantity;
        private string itemDescription;
        private ItemListRowStatus itemListRowStatus;
        private int requiredQuantity;
        private int rowPriority;

        #endregion Fields

        #region Properties

        [Display(Name = nameof(General.Code), ResourceType = typeof(General))]
        public string Code { get => this.code; set => this.SetProperty(ref this.code, value); }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get => this.creationDate; set => this.SetProperty(ref this.creationDate, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowDispatchedQuantity), ResourceType = typeof(BusinessObjects))]
        public int DispatchedQuantity { get => this.dispatchedQuantity; set => this.SetProperty(ref this.dispatchedQuantity, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowItemDescription), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get => this.itemDescription; set => this.SetProperty(ref this.itemDescription, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusDescription), ResourceType = typeof(BusinessObjects))]
        public ItemListRowStatus ItemListRowStatus { get => this.itemListRowStatus; set => this.SetProperty(ref this.itemListRowStatus, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowRequiredQuantity), ResourceType = typeof(BusinessObjects))]
        public int RequiredQuantity { get => this.requiredQuantity; set => this.SetProperty(ref this.requiredQuantity, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowPriority), ResourceType = typeof(BusinessObjects))]
        public int RowPriority { get => this.rowPriority; set => this.SetProperty(ref this.rowPriority, value); }

        #endregion Properties
    }
}
