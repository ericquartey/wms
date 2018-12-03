using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class ItemListRow : BusinessObject
    {
        #region Fields

        private string code;
        private DateTime creationDate;
        private string itemDescription;
        private string itemListRowStatusDescription;
        private int requiredQUantity;
        private int rowPriority;

        #endregion Fields

        #region Properties

        [Display(Name = nameof(General.Code), ResourceType = typeof(General))]
        public string Code { get => this.code; set => this.SetProperty(ref this.code, value); }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get => this.creationDate; set => this.SetProperty(ref this.creationDate, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowItemDescription), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get => this.itemDescription; set => this.SetProperty(ref this.itemDescription, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusDescription), ResourceType = typeof(BusinessObjects))]
        public string ItemListRowStatusDescription { get => this.itemListRowStatusDescription; set => this.SetProperty(ref this.itemListRowStatusDescription, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowRequiredQuantity), ResourceType = typeof(BusinessObjects))]
        public int RequiredQuantity { get => this.requiredQUantity; set => this.SetProperty(ref this.requiredQUantity, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowPriority), ResourceType = typeof(BusinessObjects))]
        public int RowPriority { get => this.rowPriority; set => this.SetProperty(ref this.rowPriority, value); }

        #endregion Properties
    }
}
