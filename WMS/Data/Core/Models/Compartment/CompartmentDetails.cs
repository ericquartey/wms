using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CompartmentDetails : BaseModel<int>
    {
        #region Fields

        private double? height;

        private int? maxCapacity;

        private int stock;

        private double? width;

        private double? xPosition;

        private double? yPosition;

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

        public double? Height
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

        public double? Width
        {
            get => this.width;
            set => this.width = CheckIfStrictlyPositive(value);
        }

        public double? XPosition
        {
            get => this.xPosition;
            set => this.xPosition = CheckIfPositive(value);
        }

        public double? YPosition
        {
            get => this.yPosition;
            set => this.yPosition = CheckIfPositive(value);
        }

        #endregion

        #region Methods

        public bool CanAddToLoadingUnit(IEnumerable<CompartmentDetails> compartments, LoadingUnitDetails loadingUnit)
        {
            if (loadingUnit == null)
            {
                return false;
            }

            return (loadingUnit.LoadingUnitTypeHasCompartments
                    &&
                    this.XPosition + this.Width <= loadingUnit.Width
                    &&
                    this.YPosition + this.Height <= loadingUnit.Length
                    &&
                    !compartments.Any(c => HasCollision(c, this)))
                    ||
                    (
                    !loadingUnit.LoadingUnitTypeHasCompartments &&
                    !this.XPosition.HasValue &&
                    !this.YPosition.HasValue);
        }

        public string CheckCompartment()
        {
            var sb = new StringBuilder();

            if (this.XPosition.HasValue == false)
            {
                sb.AppendLine(Errors.CompartmentXPositionIsNotSpecified);
            }

            if (this.YPosition.HasValue == false)
            {
                sb.AppendLine(Errors.CompartmentYPositionIsNotSpecified);
            }

            if (this.Width.HasValue == false)
            {
                sb.AppendLine(Errors.CompartmentSizeIsNotSpecified);
            }

            if (this.Height.HasValue == false)
            {
                sb.AppendLine(Errors.CompartmentSizeIsNotSpecified);
            }

            if (this.maxCapacity.HasValue && this.maxCapacity.Value < this.stock)
            {
                sb.AppendLine(Errors.CompartmentStockGreaterThanMaxCapacity);
            }

            return sb.ToString();
        }

        private static bool HasCollision(CompartmentDetails c1, CompartmentDetails c2)
        {
            if (c1.Id == c2.Id)
            {
                return false;
            }

            var xAPositionFinal = c1.XPosition + c1.Width;
            var yAPositionFinal = c1.YPosition + c1.Height;

            var xBPositionFinal = c2.XPosition + c2.Width;
            var yBPositionFinal = c2.YPosition + c2.Height;

            // A: Top-Left
            if (c1.XPosition >= c2.XPosition
                && c1.XPosition < xBPositionFinal
                && c1.YPosition >= c2.YPosition
                && c1.YPosition < yBPositionFinal)
            {
                return true;
            }

            // B: Top-Right
            if (xAPositionFinal > c2.XPosition
                && xAPositionFinal <= xBPositionFinal
                && c1.YPosition >= c2.YPosition
                && c1.YPosition < yBPositionFinal)
            {
                return true;
            }

            // C: Bottom-Left
            if (c1.XPosition >= c2.XPosition
                && c1.XPosition < xBPositionFinal
                && yAPositionFinal > c2.YPosition
                && yAPositionFinal <= yBPositionFinal)
            {
                return true;
            }

            // D: Bottom-Right
            if (xAPositionFinal > c2.XPosition
                && xAPositionFinal <= xBPositionFinal
                && yAPositionFinal > c2.YPosition
                && yAPositionFinal <= yBPositionFinal)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
