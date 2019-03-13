using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ferretto.Common.Interface;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public sealed class CompartmentDetails : BusinessObject, ICompartment,
        ICanDelete
    {
        #region Fields

        private IEnumerable<Enumeration> compartmentStatusChoices;

        private int? compartmentStatusId;

        private IEnumerable<Enumeration> compartmentTypeChoices;

        private int compartmentTypeId;

        private int? fifoTime;

        private int? height;

        private bool isItemPairingFixed;

        private string itemCode;

        private string itemDescription;

        private int? itemId;

        private string itemMeasureUnit;

        private string loadingUnitCode;

        private int loadingUnitId;

        private string lot;

        private IEnumerable<Enumeration> materialStatusChoices;

        private int? materialStatusId;

        private int? maxCapacity;

        private IEnumerable<Enumeration> packageTypeChoices;

        private int? packageTypeId;

        private string registrationNumber;

        private int reservedForPick;

        private int reservedToStore;

        private int stock;

        private string sub1;

        private string sub2;

        private int? width;

        private int? xPosition;

        private int? yPosition;

        #endregion

        #region Properties

        public bool CanDelete { get; set; }

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
        public int CompartmentTypeId
        {
            get => this.compartmentTypeId;
            set => this.SetProperty(ref this.compartmentTypeId, value);
        }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        public override string Error => string.Join(Environment.NewLine, new[]
                            {
                this[nameof(this.XPosition)],
                this[nameof(this.YPosition)],
                this[nameof(this.Width)],
                this[nameof(this.Height)],
                this[nameof(this.MaxCapacity)],
                this[nameof(this.Stock)],
            }
            .Distinct()
            .Where(s => !string.IsNullOrEmpty(s)));

        [Display(Name = nameof(BusinessObjects.CompartmentFifoTime), ResourceType = typeof(BusinessObjects))]
        public int? FifoTime
        {
            get => this.fifoTime;
            set => this.SetIfPositive(ref this.fifoTime, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentFirstStoreDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? FirstStoreDate { get; set; }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentHeight), ResourceType = typeof(BusinessObjects))]
        public int? Height
        {
            get => this.height;

            set
            {
                if (this.SetIfStrictlyPositive(ref this.height, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        [Display(Name = nameof(BusinessObjects.CompartmentLastInventoryDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? InventoryDate { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentIsItemPairingFixed), ResourceType = typeof(BusinessObjects))]
        public bool IsItemPairingFixed
        {
            get => this.isItemPairingFixed;
            set => this.SetProperty(ref this.isItemPairingFixed, value);
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
            set => this.SetProperty(ref this.itemId, value);
        }

        public string ItemMeasureUnit
        {
            get => this.itemMeasureUnit;
            set => this.SetProperty(ref this.itemMeasureUnit, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentLastHandlingDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastHandlingDate { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentLastPickDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPickDate { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentLastStoreDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastStoreDate { get; set; }

        public LoadingUnitDetails LoadingUnit { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCode_extended), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitCode
        {
            get => this.loadingUnitCode;
            set => this.SetProperty(ref this.loadingUnitCode, value);
        }

        public bool LoadingUnitHasCompartments { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnit), ResourceType = typeof(BusinessObjects))]
        public int LoadingUnitId
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
            set
            {
                if (this.SetProperty(ref this.materialStatusChoices, value))
                {
                    this.MaterialStatusId = this.MaterialStatusId ?? this.MaterialStatusChoices.FirstOrDefault()?.Id;
                }
            }
        }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public int? MaterialStatusId
        {
            get => this.materialStatusId;
            set => this.SetProperty(ref this.materialStatusId, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentMaxCapacity), ResourceType = typeof(BusinessObjects))]
        public int? MaxCapacity
        {
            get => this.maxCapacity;
            set
            {
                if (this.SetIfStrictlyPositive(ref this.maxCapacity, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
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
            set => this.SetProperty(ref this.registrationNumber, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentReservedForPick), ResourceType = typeof(BusinessObjects))]
        public int ReservedForPick
        {
            get => this.reservedForPick;
            set => this.SetIfPositive(ref this.reservedForPick, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentReservedToStore), ResourceType = typeof(BusinessObjects))]
        public int ReservedToStore
        {
            get => this.reservedToStore;
            set => this.SetIfPositive(ref this.reservedToStore, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentStock), ResourceType = typeof(BusinessObjects))]
        public int Stock
        {
            get => this.stock;
            set
            {
                if (this.SetIfPositive(ref this.stock, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
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
        public int? Width
        {
            get => this.width;
            set
            {
                if (this.SetIfStrictlyPositive(ref this.width, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentXPosition), ResourceType = typeof(BusinessObjects))]
        public int? XPosition
        {
            get => this.xPosition;
            set
            {
                if (this.SetIfPositive(ref this.xPosition, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentYPosition), ResourceType = typeof(BusinessObjects))]
        public int? YPosition
        {
            get => this.yPosition;
            set
            {
                if (this.SetIfPositive(ref this.yPosition, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        private bool CanAddToLoadingUnit => this.LoadingUnit == null || this.LoadingUnit.CanAddCompartment(this);

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.XPosition):
                        if (this.CanAddToLoadingUnit == false)
                        {
                            return Errors.CompartmentOverlaps;
                        }

                        if (this.XPosition.HasValue == false)
                        {
                            return Errors.CompartmentXPositionIsNotSpecified;
                        }

                        break;

                    case nameof(this.YPosition):
                        if (this.CanAddToLoadingUnit == false)
                        {
                            return Errors.CompartmentOverlaps;
                        }

                        if (this.YPosition.HasValue == false)
                        {
                            return Errors.CompartmentYPositionIsNotSpecified;
                        }

                        break;

                    case nameof(this.Width):
                        if (this.CanAddToLoadingUnit == false)
                        {
                            return Errors.CompartmentOverlaps;
                        }

                        if (this.Width.HasValue == false)
                        {
                            return Errors.CompartmentSizeIsNotSpecified;
                        }

                        break;

                    case nameof(this.Height):
                        if (this.CanAddToLoadingUnit == false)
                        {
                            return Errors.CompartmentOverlaps;
                        }

                        if (this.Height.HasValue == false)
                        {
                            return Errors.CompartmentSizeIsNotSpecified;
                        }

                        break;

                    case nameof(this.MaxCapacity):
                        if (this.maxCapacity.HasValue && this.maxCapacity.Value < this.stock)
                        {
                            return Errors.CompartmentStockGreaterThanMaxCapacity;
                        }

                        break;

                    case nameof(this.Stock):
                        if (this.maxCapacity.HasValue && this.maxCapacity.Value < this.stock)
                        {
                            return Errors.CompartmentStockGreaterThanMaxCapacity;
                        }

                        break;
                }

                return base[columnName];
            }
        }

        #endregion
    }
}
