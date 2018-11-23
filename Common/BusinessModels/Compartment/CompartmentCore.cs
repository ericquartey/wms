using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class CompartmentCore : BusinessObject, IOrderableCompartment
    {
        #region Fields

        protected int areaId;
        protected int availability;
        protected int bayId;
        protected int? fifoTime;
        protected int? height;
        protected int? maxCapacity;
        protected int reservedForPick;
        protected int reservedToStore;
        protected int stock;
        protected int? width;
        protected int? xPosition;
        protected int? yPosition;

        #endregion Fields

        #region Properties

        public int AreaId
        {
            get => this.areaId;
            set => this.SetIfPositive(ref this.areaId, value);
        }

        public int Availability
        {
            get => this.availability;
            set => this.SetIfPositive(ref this.availability, value);
        }

        public IEnumerable<Bay> Bays { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentStatus), ResourceType = typeof(BusinessObjects))]
        public int? CompartmentStatusId { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentType), ResourceType = typeof(BusinessObjects))]
        public int CompartmentTypeId { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentFifoTime), ResourceType = typeof(BusinessObjects))]
        public int? FifoTime
        {
            get => this.fifoTime;
            set => this.SetIfStrictlyPositive(ref this.fifoTime, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentFirstStoreDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? FirstStoreDate { get; set; }

        public int? Height
        {
            get => this.height;
            set => this.SetIfStrictlyPositive(ref this.height, value);
        }

        public DateTime? InventoryDate { get; set; }

        public int ItemId { get; set; }

        public int ItemPairing { get; set; }

        public DateTime? LastHandlingDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastStoreDate { get; set; }

        public int LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? MaxCapacity
        {
            get => this.maxCapacity;
            set => this.SetIfStrictlyPositive(ref this.maxCapacity, value);
        }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public int ReservedForPick
        {
            get => this.reservedForPick;
            set => this.SetIfPositive(ref this.reservedForPick, value);
        }

        public int ReservedToStore
        {
            get => this.reservedToStore;
            set => this.SetIfPositive(ref this.reservedToStore, value);
        }

        public int Stock
        {
            get => this.stock;
            set => this.SetIfPositive(ref this.stock, value);
        }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion Properties
    }
}
