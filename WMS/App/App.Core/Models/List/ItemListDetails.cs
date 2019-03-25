﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ferretto.Common.Resources;

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

        private int itemListItemsCount;

        private IEnumerable<ItemListRow> itemListRows;

        private ItemListType itemListType;

        private string itemListTypeDescription;

        private string job;

        private int priority;

        private bool shipmentUnitAssociated;

        private string shipmentUnitCode;

        private string shipmentUnitDescription;

        private ItemListStatus status;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.ItemListAreaName), ResourceType = typeof(BusinessObjects))]
        public string AreaName
        {
            get => this.areaName;
            set => this.SetProperty(ref this.areaName, value);
        }

        public bool CanAddNewRow { get; set; }

        public bool CanBeExecuted { get; set; }

        [Required]
        [Display(Name = nameof(General.Code), ResourceType = typeof(General))]
        public string Code
        {
            get => this.code;
            set
            {
                if (this.SetProperty(ref this.code, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate
        {
            get => this.creationDate;
            set => this.SetProperty(ref this.creationDate, value);
        }

        [Display(Name = nameof(General.Code), ResourceType = typeof(General))]
        public string CustomerOrderCode
        {
            get => this.customerOrderCode;
            set => this.SetProperty(ref this.customerOrderCode, value);
        }

        [Display(Name = nameof(General.Description), ResourceType = typeof(General))]
        public string CustomerOrderDescription
        {
            get => this.customerOrderDescription;
            set => this.SetProperty(ref this.customerOrderDescription, value);
        }

        [Display(Name = nameof(General.Description), ResourceType = typeof(General))]
        public string Description
        {
            get => this.description;
            set => this.SetProperty(ref this.description, value);
        }

        public override string Error => string.Join(Environment.NewLine, new[]
            {
                this[nameof(this.Code)],
                this[nameof(this.ItemListType)],
                this[nameof(this.Status)],
                this[nameof(this.Priority)],
            }
          .Distinct()
          .Where(s => !string.IsNullOrEmpty(s)));

        [Display(Name = nameof(BusinessObjects.ItemListExecutionEndDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? ExecutionEndDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListFirstExecutionDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? FirstExecutionDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListItemsCount), ResourceType = typeof(BusinessObjects))]
        public int ItemListItemsCount
        {
            get => this.itemListItemsCount;
            set => this.SetProperty(ref this.itemListItemsCount, value);
        }

        public IEnumerable<ItemListRow> ItemListRows
        {
            get => this.itemListRows;
            set => this.SetProperty(ref this.itemListRows, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListRowsCount), ResourceType = typeof(BusinessObjects))]
        public int ItemListRowsCount => this.itemListRows.Count();

        public IEnumerable<Enumeration> ItemListStatusChoices { get; set; }

        [Required]
        [Display(Name = nameof(General.Type), ResourceType = typeof(General))]
        public ItemListType ItemListType
        {
            get => this.itemListType;
            set
            {
                if (this.SetProperty(ref this.itemListType, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        [Display(Name = nameof(General.Type), ResourceType = typeof(General))]
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

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemListPriority), ResourceType = typeof(BusinessObjects))]
        public int Priority
        {
            get => this.priority;
            set => this.SetProperty(ref this.priority, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListShipmentUnitAssociated), ResourceType = typeof(BusinessObjects))]
        public bool ShipmentUnitAssociated
        {
            get => this.shipmentUnitAssociated;
            set => this.SetProperty(ref this.shipmentUnitAssociated, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListShipmentUnitCode), ResourceType = typeof(BusinessObjects))]
        public string ShipmentUnitCode
        {
            get => this.shipmentUnitCode;
            set => this.SetProperty(ref this.shipmentUnitCode, value);
        }

        [Display(Name = nameof(General.Description), ResourceType = typeof(General))]
        public string ShipmentUnitDescription
        {
            get => this.shipmentUnitDescription;
            set => this.SetProperty(ref this.shipmentUnitDescription, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemListStatus), ResourceType = typeof(BusinessObjects))]
        public ItemListStatus Status
        {
            get => this.status;
            set
            {
                if (this.SetProperty(ref this.status, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                var baseError = base[columnName];

                if (!string.IsNullOrEmpty(baseError))
                {
                    return baseError;
                }

                switch (columnName)
                {
                    case nameof(this.Priority):
                        if (this.Priority < 1)
                        {
                            return string.Format(Common.Resources.Errors.PropertyMustBeStriclyPositive, nameof(this.Priority));
                        }

                        break;

                    case nameof(this.ItemListItemsCount):
                        if (this.ItemListItemsCount < 0)
                        {
                            return string.Format(Common.Resources.Errors.PropertyMustBePositive, nameof(this.ItemListItemsCount));
                        }

                        break;
                }

                return null;
            }
        }

        #endregion
    }
}
