using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Controls.WPF;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class CompartmentDetails :
        BusinessObject,
        IDrawableCompartment,
        ITypedCompartment,
        ICapacityCompartment,
        IPairedCompartment,
        IMaterialStatusCompartment
    {
        #region Fields

        private IEnumerable<Enumeration> compartmentStatusChoices;

        private int? compartmentStatusId;

        private IEnumerable<Enumeration> compartmentTypeChoices;

        private int? compartmentTypeId;

        private double? height;

        private bool isItemPairingFixed;

        private string itemCode;

        private string itemDescription;

        private int? itemId;

        private string itemMeasureUnit;

        private string loadingUnitCode;

        private int? loadingUnitId;

        private string lot;

        private IEnumerable<Enumeration> materialStatusChoices;

        private int? materialStatusId;

        private double? maxCapacity;

        private IEnumerable<Enumeration> packageTypeChoices;

        private int? packageTypeId;

        private string registrationNumber;

        private double reservedForPick;

        private double reservedToPut;

        private double? stock;

        private string sub1;

        private string sub2;

        private double? width;

        private double? xPosition;

        private double? yPosition;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.Aisle), ResourceType = typeof(BusinessObjects))]
        public string AisleName { get; set; }

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public string AreaName { get; set; }

        public IEnumerable<Enumeration> CompartmentStatusChoices
        {
            get => this.compartmentStatusChoices;
            set => this.SetProperty(ref this.compartmentStatusChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentStatus), ResourceType = typeof(BusinessObjects))]
        public string CompartmentStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentStatus), ResourceType = typeof(BusinessObjects))]
        public int? CompartmentStatusId
        {
            get => this.compartmentStatusId;
            set => this.SetProperty(ref this.compartmentStatusId, value);
        }

        public IEnumerable<Enumeration> CompartmentTypeChoices
        {
            get => this.compartmentTypeChoices;
            set => this.SetProperty(ref this.compartmentTypeChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentType), ResourceType = typeof(BusinessObjects))]
        public int? CompartmentTypeId
        {
            get => this.compartmentTypeId;
            set => this.SetProperty(ref this.compartmentTypeId, value);
        }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentFifoStartDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? FifoStartDate { get; set; }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentHeight), ResourceType = typeof(BusinessObjects))]
        public double? Height
        {
            get => this.height;

            set => this.SetProperty(ref this.height, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentLastInventoryDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? InventoryDate { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentIsItemPairingFixed), ResourceType = typeof(BusinessObjects))]
        public bool IsItemPairingFixed
        {
            get => this.isItemPairingFixed;
            set
            {
                if (this.SetProperty(ref this.isItemPairingFixed, value))
                {
                    this.RaisePropertyChanged(nameof(this.Stock));
                }
            }
        }

        [Display(Name = nameof(BusinessObjects.ItemCode_extended), ResourceType = typeof(BusinessObjects))]
        public string ItemCode
        {
            get => this.itemCode;
            set => this.SetProperty(ref this.itemCode, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemDescription_extended), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription
        {
            get => this.itemDescription;
            set => this.SetProperty(ref this.itemDescription, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentItem), ResourceType = typeof(BusinessObjects))]
        public int? ItemId
        {
            get => this.itemId;
            set
            {
                if (this.SetProperty(ref this.itemId, value) && value == null)
                {
                    this.ClearItemRelatedInfo();
                }
            }
        }

        public string ItemMeasureUnit
        {
            get => this.itemMeasureUnit;
            set => this.SetProperty(ref this.itemMeasureUnit, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentLastPickDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPickDate { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentLastPutDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPutDate { get; set; }

        public LoadingUnitDetails LoadingUnit { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCode_extended), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitCode
        {
            get => this.loadingUnitCode;
            set => this.SetProperty(ref this.loadingUnitCode, value);
        }

        public bool LoadingUnitHasCompartments { get; set; }

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnit), ResourceType = typeof(BusinessObjects))]
        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set => this.SetProperty(ref this.loadingUnitId, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentLot), ResourceType = typeof(BusinessObjects))]
        public string Lot
        {
            get => this.lot;
            set => this.SetProperty(ref this.lot, value);
        }

        public IEnumerable<Enumeration> MaterialStatusChoices
        {
            get => this.materialStatusChoices;
            set => this.SetProperty(ref this.materialStatusChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public int? MaterialStatusId
        {
            get => this.materialStatusId;
            set => this.SetProperty(ref this.materialStatusId, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentMaxCapacity), ResourceType = typeof(BusinessObjects))]
        public double? MaxCapacity
        {
            get => this.maxCapacity;
            set
            {
                if (this.SetProperty(ref this.maxCapacity, value))
                {
                    this.RaisePropertyChanged(nameof(this.Stock));
                }
            }
        }

        public IEnumerable<Enumeration> PackageTypeChoices
        {
            get => this.packageTypeChoices;
            set => this.SetProperty(ref this.packageTypeChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.PackageType), ResourceType = typeof(BusinessObjects))]
        public int? PackageTypeId
        {
            get => this.packageTypeId;
            set => this.SetProperty(ref this.packageTypeId, value);
        }

        [Display(Name = nameof(BusinessObjects.RegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber
        {
            get => this.registrationNumber;
            set
            {
                if (this.SetProperty(ref this.registrationNumber, value))
                {
                    this.RaisePropertyChanged(nameof(this.Stock));
                }
            }
        }

        [Display(Name = nameof(BusinessObjects.CompartmentReservedForPick), ResourceType = typeof(BusinessObjects))]
        public double ReservedForPick
        {
            get => this.reservedForPick;
            set => this.SetProperty(ref this.reservedForPick, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentReservedToPut), ResourceType = typeof(BusinessObjects))]
        public double ReservedToPut
        {
            get => this.reservedToPut;
            set => this.SetProperty(ref this.reservedToPut, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentStock), ResourceType = typeof(BusinessObjects))]
        public double? Stock
        {
            get => this.stock;
            set
            {
                if (this.SetProperty(ref this.stock, value))
                {
                    this.RaisePropertyChanged(nameof(this.MaxCapacity));
                    this.RaisePropertyChanged(nameof(this.RegistrationNumber));
                    this.RaisePropertyChanged(nameof(this.IsItemPairingFixed));
                }
            }
        }

        [Display(Name = nameof(BusinessObjects.CompartmentSub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1
        {
            get => this.sub1;
            set => this.SetProperty(ref this.sub1, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentSub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2
        {
            get => this.sub2;
            set => this.SetProperty(ref this.sub2, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentWidth), ResourceType = typeof(BusinessObjects))]
        public double? Width
        {
            get => this.width;
            set => this.SetProperty(ref this.width, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentXPosition), ResourceType = typeof(BusinessObjects))]
        public double? XPosition
        {
            get => this.xPosition;
            set => this.SetProperty(ref this.xPosition, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentYPosition), ResourceType = typeof(BusinessObjects))]
        public double? YPosition
        {
            get => this.yPosition;
            set => this.SetProperty(ref this.yPosition, value);
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

                return this.GetValidationMessage(columnName);
            }
        }

        #endregion

        #region Methods

        private void ClearItemRelatedInfo()
        {
            this.MaterialStatusId = null;
            this.MaxCapacity = null;
            this.Lot = null;
            this.RegistrationNumber = null;
            this.PackageTypeId = null;
            this.Sub1 = null;
            this.Sub2 = null;
            this.Stock = null;
            this.IsItemPairingFixed = false;
        }

        private string GetValidationMessage(string columnName)
        {
            switch (columnName)
            {
                case nameof(this.XPosition):
                    return this.GetErrorMessageIfNegative(this.XPosition, columnName);

                case nameof(this.YPosition):
                    return this.GetErrorMessageIfNegative(this.YPosition, columnName);

                case nameof(this.Width):
                    return this.GetErrorMessageIfNegativeOrZero(this.Width, columnName);

                case nameof(this.Height):
                    return this.GetErrorMessageIfNegativeOrZero(this.Height, columnName);

                case nameof(this.ReservedForPick):
                    return this.GetErrorMessageIfNegative(this.ReservedForPick, columnName);

                case nameof(this.ReservedToPut):
                    return this.GetErrorMessageIfNegative(this.ReservedToPut, columnName);

                case nameof(this.RegistrationNumber):
                    if (this.ItemId.HasValue
                        && this.Stock.HasValue
                        && this.stock.Value > 1
                        && !string.IsNullOrEmpty(this.RegistrationNumber))
                    {
                        return Errors.QuantityMustBeOneIfRegistrationNumber;
                    }

                    break;

                case nameof(this.IsItemPairingFixed):
                    if (this.stock.HasValue
                        && this.stock.Value.Equals(0)
                        && !this.IsItemPairingFixed)
                    {
                        return Errors.CompartmentStockCannotBeZeroWhenItemPairingIsNotFixed;
                    }

                    break;

                case nameof(this.MaxCapacity):
                    return this.GetValidationMessageForMaxCapacity(columnName);

                case nameof(this.Stock):
                    return this.GetValidationMessageForStock(columnName);
            }

            return null;
        }

        private string GetValidationMessageForMaxCapacity(string columnName)
        {
            if (this.ItemId.HasValue && !this.MaxCapacity.HasValue)
            {
                return Errors.CompartmentMaxCapacityRequiredWhenItemIsSpecified;
            }

            if (this.MaxCapacity.HasValue && this.MaxCapacity.Value < this.stock)
            {
                return Errors.CompartmentStockGreaterThanMaxCapacity;
            }

            return this.GetErrorMessageIfNegativeOrZero(this.MaxCapacity, columnName);
        }

        private string GetValidationMessageForStock(string columnName)
        {
            if (!this.ItemId.HasValue)
            {
                return null;
            }

            if (!this.Stock.HasValue)
            {
                return Errors.CompartmentStockRequiredWhenItemIsSpecified;
            }

            if (this.maxCapacity.HasValue && this.maxCapacity.Value < this.Stock)
            {
                return Errors.CompartmentStockGreaterThanMaxCapacity;
            }

            if (this.stock.Value > 1
                && !string.IsNullOrEmpty(this.RegistrationNumber))
            {
                return Errors.QuantityMustBeOneIfRegistrationNumber;
            }

            if (!this.IsItemPairingFixed
                && this.stock.Value.Equals(0))
            {
                return Errors.CompartmentStockCannotBeZeroWhenItemPairingIsNotFixed;
            }

            return this.GetErrorMessageIfNegative(this.Stock, columnName);
        }

        #endregion
    }
}
