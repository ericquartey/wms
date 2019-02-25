using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CompartmentDetails : BaseModel<int>
    {
        #region Fields

        private int? height;

        private int? maxCapacity;

        private int stock;

        private int? width;

        private int? xPosition;

        private int? yPosition;

        #endregion

        #region Properties

        public int AllowedItemsCount { get; set; }

        public string CompartmentStatusDescription { get; set; }

        public int? CompartmentStatusId { get; set; }

        public int CompartmentTypeId { get; set; }

        public DateTime CreationDate { get; set; }

        public int? FifoTime { get; set; }

        public DateTime? FirstStoreDate { get; set; }

        public bool HasRotation { get; set; }

        public int? Height
        {
            get => this.height;
            set => this.height = CheckIfStrictlyPositive(value);
        }

        public DateTime? InventoryDate { get; set; }

        public bool IsItemPairingFixed { get; set; }

        public string ItemCode { get; set; }

        public string ItemDescription { get; set; }

        public int? ItemId { get; set; }

        public string ItemMeasureUnit { get; set; }

        public DateTime? LastHandlingDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastStoreDate { get; set; }

        public string LoadingUnitCode { get; set; }

        public bool LoadingUnitHasCompartments { get; set; }

        public int LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? MaxCapacity
        {
            get => this.maxCapacity;
            set => this.maxCapacity = CheckIfStrictlyPositive(value);
        }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public int ReservedForPick { get; set; }

        public int ReservedToStore { get; set; }

        public int Stock
        {
            get => this.stock;
            set => this.stock = CheckIfPositive(value);
        }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public int? Width
        {
            get => this.width;
            set => this.width = CheckIfStrictlyPositive(value);
        }

        public int? XPosition
        {
            get => this.xPosition;
            set => this.xPosition = CheckIfPositive(value);
        }

        public int? YPosition
        {
            get => this.yPosition;
            set => this.yPosition = CheckIfPositive(value);
        }

        #endregion
    }
}
