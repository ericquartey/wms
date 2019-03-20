using System;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BusinessModels.Interfaces;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class ItemListRow : BusinessObject,
        ICanDelete, ICanBeExecuted
    {
        #region Fields

        private string code;

        private DateTime creationDate;

        private int dispatchedQuantity;

        private string itemDescription;

        private ItemListRowStatus itemListRowStatus;

        private string itemUnitMeasure;

        private string materialStatusDescription;

        private int requestedQuantity;

        private int rowPriority;

        #endregion

        #region Properties

        public bool CanBeExecuted => this.itemListRowStatus == ItemListRowStatus.Incomplete
                    || this.itemListRowStatus == ItemListRowStatus.Suspended
                    || this.itemListRowStatus == ItemListRowStatus.Waiting;

        public bool CanDelete { get; set; }

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

        public string ItemUnitMeasure { get => this.itemUnitMeasure; set => this.SetProperty(ref this.itemUnitMeasure, value); }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public string MaterialStatusDescription { get => this.materialStatusDescription; set => this.SetProperty(ref this.materialStatusDescription, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowRequestedQuantity), ResourceType = typeof(BusinessObjects))]
        public int RequestedQuantity { get => this.requestedQuantity; set => this.SetProperty(ref this.requestedQuantity, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowPriority), ResourceType = typeof(BusinessObjects))]
        public int RowPriority { get => this.rowPriority; set => this.SetProperty(ref this.rowPriority, value); }

        #endregion
    }
}
