using System;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class Compartment : BusinessObject
    {
        #region Fields

        private int? fifoTime;
        private int? height;
        private int? maxCapacity;
        private int reservedForPick;
        private int reservedToStore;
        private int stock;
        private int? width;
        private int? xPosition;
        private int? yPosition;

        #endregion Fields

        #region Properties

        public string Code { get; set; }
        public string CompartmentStatusDescription { get; set; }
        public string CompartmentTypeDescription { get; set; }
        public DateTime CreationDate { get; set; }

        public int? FifoTime
        {
            get => this.fifoTime;
            set => SetIfPositive(ref this.fifoTime, value);
        }

        public DateTime? FirstStoreDate { get; set; }

        public int? Height
        {
            get => this.height;
            set => SetIfPositive(ref this.height, value);
        }

        public int Id { get; set; }
        public DateTime? InventoryDate { get; set; }
        public string ItemDescription { get; set; }
        public string ItemPairing { get; set; }
        public int? LaserPointerCoordinate1 { get; set; }
        public int? LaserPointerCoordinate2 { get; set; }
        public DateTime? LastHandlingDate { get; set; }
        public DateTime? LastPickDate { get; set; }
        public DateTime? LastStoreDate { get; set; }
        public string LoadingUnitCode { get; set; }
        public string Lot { get; set; }
        public string MaterialStatusDescription { get; set; }

        public int? MaxCapacity
        {
            get => this.maxCapacity;
            set => SetIfPositive(ref this.maxCapacity, value);
        }

        public string PackageTypeDescription { get; set; }
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

        public int? Width
        {
            get => this.width;
            set => SetIfStrictlyPositive(ref this.width, value);
        }

        public int? XPosition
        {
            get => this.xPosition;
            set => SetIfPositive(ref this.xPosition, value);
        }

        public int? YPosition
        {
            get => this.yPosition;
            set => SetIfPositive(ref this.yPosition, value);
        }

        #endregion Properties
    }
}
