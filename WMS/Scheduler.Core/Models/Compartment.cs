using System;
using System.Collections.Generic;

namespace Ferretto.WMS.Scheduler.Core
{
    public class Compartment : BusinessObject, IOrderableCompartment
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
            set => this.areaId = value;
        }

        public int Availability => this.stock - this.reservedForPick + this.reservedToStore;

        public IEnumerable<Bay> Bays { get; set; }

        public int CellId { get; set; }

        public string Code { get; set; }

        public int? CompartmentStatusId { get; set; }

        public int CompartmentTypeId { get; set; }

        public DateTime CreationDate { get; set; }

        public int? FifoTime
        {
            get => this.fifoTime;
            set => this.SetIfStrictlyPositive(ref this.fifoTime, value);
        }

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
            set => SetIfPositive(ref this.reservedForPick, value);
        }

        public int ReservedToStore
        {
            get => this.reservedToStore;
            set => SetIfPositive(ref this.reservedToStore, value);
        }

        public int Stock
        {
            get => this.stock;
            set => SetIfPositive(ref this.stock, value);
        }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion Properties
    }
}
