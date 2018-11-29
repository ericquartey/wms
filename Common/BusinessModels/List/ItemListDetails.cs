using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class ItemListDetails : BusinessObject
    {
        #region Fields

        private string areaName;
        private string code;
        private DateTime creationDate;
        private string customerOrderCode;
        private string customerOrderDescription;
        private string description;
        private int itemListItemsCount;
        private int itemListRowsCount;
        private string itemListStatusDescription;
        private string itemListTypeDescription;
        private string job;
        private int priority;
        private bool shipmentUnitAssociated;
        private string shipmentUnitCode;
        private string shipmentUnitDescription;

        #endregion Fields

        #region Properties

        [Display(Name = nameof(BusinessObjects.ItemListAreaName), ResourceType = typeof(BusinessObjects))]
        public string AreaName { get => this.areaName; set => this.SetProperty(ref this.areaName, value); }

        [Display(Name = nameof(BusinessObjects.ItemListCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get => this.code; set => this.SetProperty(ref this.code, value); }

        [Display(Name = nameof(BusinessObjects.ItemListCreationDate), ResourceType = typeof(BusinessObjects))]
        public DateTime CreationDate { get => this.creationDate; set => this.SetProperty(ref this.creationDate, value); }

        [Display(Name = nameof(BusinessObjects.ItemListCustomerOrderCode), ResourceType = typeof(BusinessObjects))]
        public string CustomerOrderCode { get => this.customerOrderCode; set => this.SetProperty(ref this.customerOrderCode, value); }

        [Display(Name = nameof(BusinessObjects.ItemListCustomerOrderDescription), ResourceType = typeof(BusinessObjects))]
        public string CustomerOrderDescription { get => this.customerOrderDescription; set => this.SetProperty(ref this.customerOrderDescription, value); }

        [Display(Name = nameof(BusinessObjects.ItemListDescription), ResourceType = typeof(BusinessObjects))]
        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        [Display(Name = nameof(BusinessObjects.ItemListExecutionEndDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? ExecutionEndDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListFireExecutionDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? FireExecutionDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListItemsCount), ResourceType = typeof(BusinessObjects))]
        public int ItemListItemsCount
        {
            get => this.itemListItemsCount;
            set => this.SetProperty(ref this.itemListItemsCount, value);
        }

        public IEnumerable<ItemListRow> ItemListRows { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListRowsCount), ResourceType = typeof(BusinessObjects))]
        public int ItemListRowsCount { get => this.itemListRowsCount; set => this.SetProperty(ref this.itemListRowsCount, value); }

        public IEnumerable<Enumeration> ItemListStatusChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListStatusDescription), ResourceType = typeof(BusinessObjects))]
        public string ItemListStatusDescription { get => this.itemListStatusDescription; set => this.SetProperty(ref this.itemListStatusDescription, value); }

        public int ItemListStatusId { get; set; }

        public IEnumerable<Enumeration> ItemListTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListTypeDescription), ResourceType = typeof(BusinessObjects))]
        public string ItemListTypeDescription { get => this.itemListTypeDescription; set => this.SetProperty(ref this.itemListTypeDescription, value); }

        [Display(Name = nameof(BusinessObjects.ItemListTypeId), ResourceType = typeof(BusinessObjects))]
        public int ItemListTypeId { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListJob), ResourceType = typeof(BusinessObjects))]
        public string Job { get => this.job; set => this.SetProperty(ref this.job, value); }

        [Display(Name = nameof(BusinessObjects.ItemListLastModificationDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastModificationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListPriority), ResourceType = typeof(BusinessObjects))]
        public int Priority { get => this.priority; set => this.SetProperty(ref this.priority, value); }

        [Display(Name = nameof(BusinessObjects.ItemListShipmentUnitAssociated), ResourceType = typeof(BusinessObjects))]
        public bool ShipmentUnitAssociated { get => this.shipmentUnitAssociated; set => this.SetProperty(ref this.shipmentUnitAssociated, value); }

        [Display(Name = nameof(BusinessObjects.ItemListShipmentUnitCode), ResourceType = typeof(BusinessObjects))]
        public string ShipmentUnitCode { get => this.shipmentUnitCode; set => this.SetProperty(ref this.shipmentUnitCode, value); }

        [Display(Name = nameof(BusinessObjects.ItemListShipmentUnitDescription), ResourceType = typeof(BusinessObjects))]
        public string ShipmentUnitDescription { get => this.shipmentUnitDescription; set => this.SetProperty(ref this.shipmentUnitDescription, value); }

        #endregion Properties
    }
}
