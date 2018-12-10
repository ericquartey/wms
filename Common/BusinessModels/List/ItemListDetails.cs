using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        private IEnumerable<ItemListRow> itemListRows;
        private ItemListStatus itemListStatus;
        private int itemListType;
        private string job;
        private int priority;
        private bool shipmentUnitAssociated;
        private string shipmentUnitCode;
        private string shipmentUnitDescription;

        #endregion Fields

        #region Properties

        [Display(Name = nameof(BusinessObjects.ItemListAreaName), ResourceType = typeof(BusinessObjects))]
        public string AreaName { get => this.areaName; set => this.SetProperty(ref this.areaName, value); }

        [Display(Name = nameof(General.Code), ResourceType = typeof(General))]
        public string Code { get => this.code; set => this.SetProperty(ref this.code, value); }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get => this.creationDate; set => this.SetProperty(ref this.creationDate, value); }

        [Display(Name = nameof(General.Code), ResourceType = typeof(General))]
        public string CustomerOrderCode { get => this.customerOrderCode; set => this.SetProperty(ref this.customerOrderCode, value); }

        [Display(Name = nameof(General.Description), ResourceType = typeof(General))]
        public string CustomerOrderDescription { get => this.customerOrderDescription; set => this.SetProperty(ref this.customerOrderDescription, value); }

        [Display(Name = nameof(General.Description), ResourceType = typeof(General))]
        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        [Display(Name = nameof(BusinessObjects.ItemListExecutionEndDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? ExecutionEndDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListFirstExecutionDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? FirstExecutionDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListItemsCount), ResourceType = typeof(BusinessObjects))]
        public int ItemListItemsCount
        {
            get => this.itemListItemsCount;
            set => this.SetIfPositive(ref this.itemListItemsCount, value);
        }

        public IEnumerable<ItemListRow> ItemListRows
        {
            get => this.itemListRows;
            set => this.SetProperty(ref this.itemListRows, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListRowsCount), ResourceType = typeof(BusinessObjects))]
        public int ItemListRowsCount
        {
            get => this.itemListRows.Count();
        }

        [Display(Name = nameof(BusinessObjects.ItemListStatus), ResourceType = typeof(BusinessObjects))]
        public ItemListStatus ItemListStatus { get => this.itemListStatus; set => this.SetProperty(ref this.itemListStatus, value); }

        public IEnumerable<Enumeration> ItemListStatusChoices { get; set; }

        [Display(Name = nameof(General.Type), ResourceType = typeof(General))]
        public int ItemListType
        {
            get => this.itemListType;
            set => this.SetProperty(ref this.itemListType, value);
        }

        public IEnumerable<Enumeration> ItemListTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListJob), ResourceType = typeof(BusinessObjects))]
        public string Job { get => this.job; set => this.SetProperty(ref this.job, value); }

        [Display(Name = nameof(General.LastModificationDate), ResourceType = typeof(General))]
        public DateTime? LastModificationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListPriority), ResourceType = typeof(BusinessObjects))]
        public int Priority { get => this.priority; set => this.SetProperty(ref this.priority, value); }

        [Display(Name = nameof(BusinessObjects.ItemListShipmentUnitAssociated), ResourceType = typeof(BusinessObjects))]
        public bool ShipmentUnitAssociated { get => this.shipmentUnitAssociated; set => this.SetProperty(ref this.shipmentUnitAssociated, value); }

        [Display(Name = nameof(BusinessObjects.ItemListShipmentUnitCode), ResourceType = typeof(BusinessObjects))]
        public string ShipmentUnitCode { get => this.shipmentUnitCode; set => this.SetProperty(ref this.shipmentUnitCode, value); }

        [Display(Name = nameof(General.Description), ResourceType = typeof(General))]
        public string ShipmentUnitDescription { get => this.shipmentUnitDescription; set => this.SetProperty(ref this.shipmentUnitDescription, value); }

        #endregion Properties
    }
}
