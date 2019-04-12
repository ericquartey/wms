using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ferretto.Common.Controls.WPF;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.App.Core.Models
{
    [Resource(nameof(Data.WebAPI.Contracts.Compartment))]
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

        private int compartmentTypeId;

        private int? fifoTime;

        private double? height;

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

        private double? width;

        private double? xPosition;

        private double? yPosition;

        #endregion

        #region Properties

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
                this[nameof(this.ReservedForPick)],
                this[nameof(this.ReservedToStore)],
                this[nameof(this.FifoTime)],
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
            set => this.SetProperty(ref this.fifoTime, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentFirstStoreDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? FirstStoreDate { get; set; }

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
            set => this.SetProperty(ref this.maxCapacity, value);
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
            set => this.SetProperty(ref this.reservedForPick, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentReservedToStore), ResourceType = typeof(BusinessObjects))]
        public int ReservedToStore
        {
            get => this.reservedToStore;
            set => this.SetProperty(ref this.reservedToStore, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentStock), ResourceType = typeof(BusinessObjects))]
        public int Stock
        {
            get => this.stock;
            set => this.SetProperty(ref this.stock, value);
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
                var baseError = base[columnName];
                if (!string.IsNullOrEmpty(baseError))
                {
                    return baseError;
                }

                switch (columnName)
                {
                    case nameof(this.XPosition):
                        return GetErrorMessageIfNegative(this.XPosition, nameof(this.XPosition));

                    case nameof(this.YPosition):
                        return GetErrorMessageIfNegative(this.YPosition, nameof(this.YPosition));

                    case nameof(this.Width):
                        return GetErrorMessageIfNegativeOrZero(this.Width, nameof(this.Width));

                    case nameof(this.Height):
                        return GetErrorMessageIfNegative(this.Height, nameof(this.Height));

                    case nameof(this.ReservedForPick):
                        return GetErrorMessageIfNegative(this.ReservedForPick, nameof(this.ReservedForPick));

                    case nameof(this.ReservedToStore):
                        return GetErrorMessageIfNegative(this.ReservedToStore, nameof(this.ReservedToStore));

                    case nameof(this.MaxCapacity):
                        if (this.MaxCapacity.HasValue && this.MaxCapacity.Value < this.stock)
                        {
                            return Errors.CompartmentStockGreaterThanMaxCapacity;
                        }

                        return GetErrorMessageIfNegative(this.MaxCapacity, nameof(this.MaxCapacity));

                    case nameof(this.FifoTime):
                        return GetErrorMessageIfNegative(this.FifoTime, nameof(this.FifoTime));

                    case nameof(this.Stock):
                        if (this.maxCapacity.HasValue && this.maxCapacity.Value < this.Stock)
                        {
                            return Errors.CompartmentStockGreaterThanMaxCapacity;
                        }

                        return GetErrorMessageIfNegative(this.Stock, nameof(this.Stock));
                }

                return base[columnName];
            }
        }

        #endregion
    }
}
