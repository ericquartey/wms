using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class ItemListRowDetails : BusinessObject
    {
        #region Fields

        private string code;

        private DateTime? completionDate;

        private DateTime creationDate;

        private double dispatchedQuantity;

        private string itemDescription;

        private int itemId;

        private string itemListCode;

        private string itemListDescription;

        private int itemListId;

        private ItemListType itemListType;

        private string itemUnitMeasure;

        private DateTime? lastExecutionDate;

        private DateTime? lastModificationDate;

        private ItemListStatus listStatus;

        private string lot;

        private IEnumerable<Enumeration> materialStatusChoices;

        private int? materialStatusId;

        private IEnumerable<Enumeration> packageTypeChoices;

        private int? packageTypeId;

        private int? priority;

        private string registrationNumber;

        private double requestedQuantity;

        private ItemListRowStatus status;

        private string sub1;

        private string sub2;

        #endregion

        #region Properties

        [Required]
        [Display(Name = nameof(General.Code), ResourceType = typeof(General))]
        public string Code { get => this.code; set => this.SetProperty(ref this.code, value); }

        [Display(Name = nameof(BusinessObjects.CompletionDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? CompletionDate { get => this.completionDate; set => this.SetProperty(ref this.completionDate, value); }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get => this.creationDate; set => this.SetProperty(ref this.creationDate, value); }

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemListRowDispatchedQuantity), ResourceType = typeof(BusinessObjects))]
        public double DispatchedQuantity { get => this.dispatchedQuantity; set => this.SetProperty(ref this.dispatchedQuantity, value); }

        public override string Error => string.Join(Environment.NewLine, new[]
        {
            this[nameof(this.DispatchedQuantity)],
            this[nameof(this.RequestedQuantity)],
            this[nameof(this.Code)],
            this[nameof(this.ItemId)],
            this[nameof(this.Status)],
        }
        .Distinct()
        .Where(s => !string.IsNullOrEmpty(s)));

        [Display(Name = nameof(BusinessObjects.ItemListRowItemDescription), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get => this.itemDescription; set => this.SetProperty(ref this.itemDescription, value); }

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemListRowItemDescription), ResourceType = typeof(BusinessObjects))]
        public int ItemId { get => this.itemId; set => this.SetProperty(ref this.itemId, value); }

        [Display(Name = nameof(General.Code), ResourceType = typeof(General))]
        public string ItemListCode { get => this.itemListCode; set => this.SetProperty(ref this.itemListCode, value); }

        [Display(Name = nameof(General.Description), ResourceType = typeof(General))]
        public string ItemListDescription { get => this.itemListDescription; set => this.SetProperty(ref this.itemListDescription, value); }

        [Display(Name = nameof(BusinessObjects.ItemList), ResourceType = typeof(BusinessObjects))]
        public int ItemListId { get => this.itemListId; set => this.SetProperty(ref this.itemListId, value); }

        [Display(Name = nameof(BusinessObjects.ItemListStatus), ResourceType = typeof(BusinessObjects))]
        public ItemListStatus ItemListStatus { get => this.listStatus; set => this.SetProperty(ref this.listStatus, value); }

        [Display(Name = nameof(General.Type), ResourceType = typeof(General))]
        public ItemListType ItemListType { get => this.itemListType; set => this.SetProperty(ref this.itemListType, value); }

        public string ItemUnitMeasure { get => this.itemUnitMeasure; set => this.SetProperty(ref this.itemUnitMeasure, value); }

        [Display(Name = nameof(BusinessObjects.LastExecutionDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastExecutionDate { get => this.lastExecutionDate; set => this.SetProperty(ref this.lastExecutionDate, value); }

        [Display(Name = nameof(General.LastModificationDate), ResourceType = typeof(General))]
        public DateTime? LastModificationDate { get => this.lastModificationDate; set => this.SetProperty(ref this.lastModificationDate, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowLot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get => this.lot; set => this.SetProperty(ref this.lot, value); }

        public IEnumerable<Enumeration> MaterialStatusChoices
        {
            get => this.materialStatusChoices;
            set => this.SetProperty(ref this.materialStatusChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public int? MaterialStatusId { get => this.materialStatusId; set => this.SetProperty(ref this.materialStatusId, value); }

        public IEnumerable<Enumeration> PackageTypeChoices
        {
            get => this.packageTypeChoices;
            set => this.SetProperty(ref this.packageTypeChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.PackageType), ResourceType = typeof(BusinessObjects))]
        public int? PackageTypeId { get => this.packageTypeId; set => this.SetProperty(ref this.packageTypeId, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowPriority), ResourceType = typeof(BusinessObjects))]
        public int? Priority { get => this.priority; set => this.SetProperty(ref this.priority, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowRegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber { get => this.registrationNumber; set => this.SetProperty(ref this.registrationNumber, value); }

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemListRowRequestedQuantity), ResourceType = typeof(BusinessObjects))]
        public double RequestedQuantity { get => this.requestedQuantity; set => this.SetProperty(ref this.requestedQuantity, value); }

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemListRowStatusDescription), ResourceType = typeof(BusinessObjects))]
        public ItemListRowStatus Status { get => this.status; set => this.SetProperty(ref this.status, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowSub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get => this.sub1; set => this.SetProperty(ref this.sub1, value); }

        [Display(Name = nameof(BusinessObjects.ItemListRowSub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get => this.sub2; set => this.SetProperty(ref this.sub2, value); }

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
                    case nameof(this.DispatchedQuantity):
                        return GetErrorMessageIfNegative(this.DispatchedQuantity, nameof(this.DispatchedQuantity));

                    case nameof(this.RequestedQuantity):
                        return GetErrorMessageIfNegative(this.RequestedQuantity, nameof(this.RequestedQuantity));

                    case nameof(this.Code):
                        break;

                    case nameof(this.ItemId):
                        return GetErrorMessageIfNegativeOrZero(this.ItemId, nameof(this.ItemId));

                    case nameof(this.Status):
                        break;
                }

                return null;
            }
        }

        #endregion
    }
}
