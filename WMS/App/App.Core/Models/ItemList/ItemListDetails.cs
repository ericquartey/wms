using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
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

        private IEnumerable<ItemListRow> itemListRows;

        private ItemListType? itemListType;

        private string itemListTypeDescription;

        private string job;

        private int? priority;

        private bool shipmentUnitAssociated;

        private string shipmentUnitCode;

        private string shipmentUnitDescription;

        private ItemListStatus? status;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public string AreaName
        {
            get => this.areaName;
            set => this.SetProperty(ref this.areaName, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.Code), ResourceType = typeof(BusinessObjects))]
        public string Code
        {
            get => this.code;
            set => this.SetProperty(ref this.code, value);
        }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate
        {
            get => this.creationDate;
            set => this.SetProperty(ref this.creationDate, value);
        }

        [Display(Name = nameof(BusinessObjects.Code), ResourceType = typeof(BusinessObjects))]
        public string CustomerOrderCode
        {
            get => this.customerOrderCode;
            set => this.SetProperty(ref this.customerOrderCode, value);
        }

        [Display(Name = nameof(BusinessObjects.Description), ResourceType = typeof(BusinessObjects))]
        public string CustomerOrderDescription
        {
            get => this.customerOrderDescription;
            set => this.SetProperty(ref this.customerOrderDescription, value);
        }

        [Display(Name = nameof(BusinessObjects.Description), ResourceType = typeof(BusinessObjects))]
        public string Description
        {
            get => this.description;
            set => this.SetProperty(ref this.description, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListExecutionEndDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? ExecutionEndDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListFirstExecutionDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? FirstExecutionDate { get; set; }

        public IEnumerable<ItemListRow> ItemListRows
        {
            get => this.itemListRows;
            set => this.SetProperty(ref this.itemListRows, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListRowsCount), ResourceType = typeof(BusinessObjects))]
        public int ItemListRowsCount => this.itemListRows.Count();

        public IEnumerable<Enumeration> ItemListStatusChoices { get; set; }

        [Required]
        [Display(Name = nameof(BusinessObjects.Type), ResourceType = typeof(BusinessObjects))]
        public ItemListType? ItemListType
        {
            get => this.itemListType;
            set => this.SetProperty(ref this.itemListType, value);
        }

        [Display(Name = nameof(BusinessObjects.Type), ResourceType = typeof(BusinessObjects))]
        public string ItemListTypeDescription
        {
            get => this.itemListTypeDescription;
            set => this.SetProperty(ref this.itemListTypeDescription, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListJob), ResourceType = typeof(BusinessObjects))]
        public string Job
        {
            get => this.job;
            set => this.SetProperty(ref this.job, value);
        }

        [Display(Name = nameof(General.LastModificationDate), ResourceType = typeof(General))]
        public DateTime? LastModificationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.Priority), ResourceType = typeof(BusinessObjects))]
        public int? Priority
        {
            get => this.priority;
            set => this.SetProperty(ref this.priority, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListShipmentUnitAssociated), ResourceType = typeof(BusinessObjects))]
        public bool ShipmentUnitAssociated
        {
            get => this.shipmentUnitAssociated;
            set
            {
                this.SetProperty(ref this.shipmentUnitAssociated, value);
                if (!value)
                {
                    this.ShipmentUnitCode = null;
                    this.ShipmentUnitDescription = null;
                }

                this.RaisePropertyChanged(nameof(this.ShipmentUnitCode));
            }
        }

        [Display(Name = nameof(BusinessObjects.Code), ResourceType = typeof(BusinessObjects))]
        public string ShipmentUnitCode
        {
            get => this.shipmentUnitCode;
            set => this.SetProperty(ref this.shipmentUnitCode, value);
        }

        [Display(Name = nameof(BusinessObjects.Description), ResourceType = typeof(BusinessObjects))]
        public string ShipmentUnitDescription
        {
            get => this.shipmentUnitDescription;
            set => this.SetProperty(ref this.shipmentUnitDescription, value);
        }

        [Display(Name = nameof(BusinessObjects.Status), ResourceType = typeof(BusinessObjects))]
        public ItemListStatus? Status
        {
            get => this.status;
            set => this.SetProperty(ref this.status, value);
        }

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                if (!this.IsValidationEnabled)
                {
                    return null;
                }

                var baseError = base[columnName];
                if (!string.IsNullOrEmpty(baseError))
                {
                    return baseError;
                }

                switch (columnName)
                {
                    case nameof(this.Priority):
                        return this.GetErrorMessageIfNegativeOrZero(this.Priority, columnName);

                    case nameof(this.ShipmentUnitCode):
                        if (this.ShipmentUnitAssociated)
                        {
                            return this.GetErrorMessageIfNullOrEmpty(this.ShipmentUnitCode, columnName);
                        }

                        break;
                }

                return null;
            }
        }

        #endregion
    }
}
