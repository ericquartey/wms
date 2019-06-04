using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.Common.Resources;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CompartmentDetails : BaseModel<int>, ICompartmentDeletePolicy, ICompartmentUpdatePolicy, ICompartmentItemDetails
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

        public string AisleName { get; set; }

        public int AllowedItemsCount { get; set; }

        public string AreaName { get; set; }

        public string CompartmentStatusDescription { get; set; }

        public int? CompartmentStatusId { get; set; }

        public int CompartmentTypeId { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? FifoStartDate { get; set; }

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

        public DateTime? LastPutDate { get; set; }

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

        public double ReservedToPut { get; set; }

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

            if (!this.XPosition.HasValue)
            {
                sb.AppendLine(Errors.CompartmentXPositionIsNotSpecified);
            }

            if (!this.YPosition.HasValue)
            {
                sb.AppendLine(Errors.CompartmentYPositionIsNotSpecified);
            }

            if (!this.Width.HasValue)
            {
                sb.AppendLine(Errors.CompartmentSizeIsNotSpecified);
            }

            if (!this.Height.HasValue)
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

            return HasCornersInCompartment(c1, c2) || HasCornersInCompartment(c2, c1);
        }

        private static bool HasCornersInCompartment(CompartmentDetails source, CompartmentDetails target)
        {
            // check if any source corner is inside target
            var sourceXPositionFinal = source.XPosition + source.Width;
            var sourceYPositionFinal = source.YPosition + source.Height;

            var targetXPositionFinal = target.XPosition + target.Width;
            var targetYPositionFinal = target.YPosition + target.Height;

            // Bottom Left
            if (source.xPosition >= target.xPosition
                && source.xPosition < targetXPositionFinal
                && source.yPosition >= target.yPosition
                && source.yPosition < targetYPositionFinal)
            {
                return true;
            }

            // Bottom Right
            if (sourceXPositionFinal > target.xPosition
                && sourceXPositionFinal <= targetXPositionFinal
                && source.yPosition >= target.yPosition
                && source.yPosition < targetYPositionFinal)
            {
                return true;
            }

            // Top Left
            if (source.xPosition >= target.xPosition
                && source.xPosition < targetXPositionFinal
                && sourceYPositionFinal > target.yPosition
                && sourceYPositionFinal <= targetYPositionFinal)
            {
                return true;
            }

            // Top Right
            if (sourceXPositionFinal > target.xPosition
                && sourceXPositionFinal <= targetXPositionFinal
                && sourceYPositionFinal > target.yPosition
                && sourceYPositionFinal <= targetYPositionFinal)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
