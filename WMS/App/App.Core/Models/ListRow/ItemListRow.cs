using System;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class ItemListRow : BusinessObject
    {
        #region Fields

        private string code;

        private DateTime creationDate;

        private int dispatchedQuantity;

        private string itemDescription;

        private string itemUnitMeasure;

        private string materialStatusDescription;

        private int? priority;

        private int requestedQuantity;

        private ItemListRowStatus status;

        #endregion

        #region Properties

        public bool CanBeExecuted { get; set; }

        [Display(Name = nameof(General.Code), ResourceType = typeof(General))]
        public string Code { get => this.code; set => this.SetProperty(ref this.code, value); }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get => this.creationDate; set => this.SetProperty(ref this.creationDate, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowDispatchedQuantity), ResourceType = typeof(BusinessObjects))]
        public int DispatchedQuantity { get => this.dispatchedQuantity; set => this.SetProperty(ref this.dispatchedQuantity, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowItemDescription), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get => this.itemDescription; set => this.SetProperty(ref this.itemDescription, value); }

        public string ItemUnitMeasure { get => this.itemUnitMeasure; set => this.SetProperty(ref this.itemUnitMeasure, value); }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public string MaterialStatusDescription { get => this.materialStatusDescription; set => this.SetProperty(ref this.materialStatusDescription, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowPriority), ResourceType = typeof(BusinessObjects))]
        public int? Priority { get => this.priority; set => this.SetProperty(ref this.priority, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowRequestedQuantity), ResourceType = typeof(BusinessObjects))]
        public int RequestedQuantity { get => this.requestedQuantity; set => this.SetProperty(ref this.requestedQuantity, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusDescription), ResourceType = typeof(BusinessObjects))]
        public ItemListRowStatus Status { get => this.status; set => this.SetProperty(ref this.status, value); }

        #endregion
    }
}
