using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CompartmentDetails : BaseModel<int>, ICompartmentDeletePolicy
    {
        #region Fields

        private double? height;

        private double? maxCapacity;

        private double stock;

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

        public double? MaxCapacity
        {
            get => this.maxCapacity;
            set => this.maxCapacity = CheckIfStrictlyPositive(value);
        }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public double ReservedForPick { get; set; }

        public double ReservedToStore { get; set; }

        public double Stock
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

        [SuppressMessage(
            "Microsoft.Maintainability",
            "S3776",
            Justification = "OK")]
        [SuppressMessage(
            "Microsoft.Maintainability",
            "CA1502",
            Justification = "OK")]
        private static bool HasCollision(CompartmentDetails c1, CompartmentDetails c2)
        {
            if (c1.Id == c2.Id)
            {
                return false;
            }

            var c1XPositionFinal = c1.XPosition + c1.Width;
            var c1YPositionFinal = c1.YPosition + c1.Height;

            var c2XPositionFinal = c2.XPosition + c2.Width;
            var c2YPositionFinal = c2.YPosition + c2.Height;

            // c1 BL->c2
            if (c1.xPosition >= c2.xPosition
                && c1.xPosition < c2XPositionFinal
                && c1.yPosition >= c2.yPosition
                && c1.yPosition < c2YPositionFinal)
            {
                return true;
            }

            // c1 BR->c2
            if (c1XPositionFinal > c2.xPosition
                && c1XPositionFinal <= c2XPositionFinal
                && c1.yPosition >= c2.yPosition
                && c1.yPosition < c2YPositionFinal)
            {
                return true;
            }

            // c1 TL->c2
            if (c1.xPosition >= c2.xPosition
                && c1.xPosition < c2XPositionFinal
                && c1YPositionFinal > c2.yPosition
                && c1YPositionFinal <= c2YPositionFinal)
            {
                return true;
            }

            // c1 TR->c2
            if (c1XPositionFinal > c2.xPosition
                && c1XPositionFinal <= c2XPositionFinal
                && c1YPositionFinal > c2.yPosition
                && c1YPositionFinal <= c2YPositionFinal)
            {
                return true;
            }

            // c2 BL->c1
            if (c2.xPosition >= c1.xPosition
                && c2.xPosition < c1XPositionFinal
                && c2.yPosition >= c1.yPosition
                && c2.yPosition < c1YPositionFinal)
            {
                return true;
            }

            // c2 BR->c1
            if (c2XPositionFinal > c1.xPosition
                && c2XPositionFinal <= c1XPositionFinal
                && c2.yPosition >= c1.yPosition
                && c2.yPosition < c1YPositionFinal)
            {
                return true;
            }

            // c2 TL->c1
            if (c2.xPosition >= c1.xPosition
                && c2.xPosition < c1XPositionFinal
                && c2YPositionFinal > c1.yPosition
                && c2YPositionFinal <= c1YPositionFinal)
            {
                return true;
            }

            // c2 TR->c1
            if (c2XPositionFinal > c1.xPosition
                && c2XPositionFinal <= c1XPositionFinal
                && c2YPositionFinal > c1.yPosition
                && c2YPositionFinal <= c1YPositionFinal)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
