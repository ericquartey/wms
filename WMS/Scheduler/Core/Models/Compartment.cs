using System;

namespace Ferretto.WMS.Scheduler.Core
{
    public class Compartment : BusinessObject, IOrderableCompartment
    {
        #region Fields

        private int? fifoTime;
        private int? maxCapacity;
        private int reservedForPick;
        private int reservedToStore;
        private int stock;

        #endregion

        #region Properties

        public int AreaId { get; set; }

        public int Availability => this.stock - this.reservedForPick + this.reservedToStore;

        public int CellId { get; set; }

        public int? FifoTime
        {
            get => this.fifoTime;
            set => SetIfStrictlyPositive(ref this.fifoTime, value);
        }

        public DateTime? FirstStoreDate { get; set; }

        public int ItemId { get; set; }

        public int LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? MaxCapacity
        {
            get => this.maxCapacity;
            set => SetIfStrictlyPositive(ref this.maxCapacity, value);
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

        #endregion
    }
}
