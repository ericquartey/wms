using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Compartment))]
    public class CompartmentDetails : BaseModel<int>, ICompartmentDeletePolicy, ICompartmentUpdatePolicy, ICompartmentItemDetails
    {
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

        [Required]
        [Positive]
        public double? Height { get; set; }

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

        [Positive]
        public double? MaxCapacity { get; set; }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        [PositiveOrZero]
        public double ReservedForPick { get; set; }

        [PositiveOrZero]
        public double ReservedToPut { get; set; }

        [PositiveOrZero]
        public double Stock { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        [Required]
        [Positive]
        public double? Width { get; set; }

        [Required]
        [PositiveOrZero]
        public double? XPosition { get; set; }

        [Required]
        [PositiveOrZero]
        public double? YPosition { get; set; }

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

        public string GetValidationMessages()
        {
            var sb = new StringBuilder();

            if (!this.XPosition.HasValue)
            {
                sb.AppendLine(Resources.Errors.CompartmentXPositionIsNotSpecified);
            }

            if (!this.YPosition.HasValue)
            {
                sb.AppendLine(Resources.Errors.CompartmentYPositionIsNotSpecified);
            }

            if (!this.Width.HasValue)
            {
                sb.AppendLine(Resources.Errors.CompartmentSizeIsNotSpecified);
            }

            if (!this.Height.HasValue)
            {
                sb.AppendLine(Resources.Errors.CompartmentSizeIsNotSpecified);
            }

            if (this.MaxCapacity.HasValue
                && this.MaxCapacity.Value < this.Stock)
            {
                sb.AppendLine(Resources.Errors.CompartmentStockGreaterThanMaxCapacity);
            }

            if (!string.IsNullOrEmpty(this.RegistrationNumber)
                && this.Stock > 1)
            {
                sb.AppendLine(Resources.Errors.QuantityMustBeOneIfRegistrationNumber);
            }

            if (this.ItemId.HasValue
                && this.Stock.Equals(0)
                && !this.IsItemPairingFixed)
            {
                sb.AppendLine(Resources.Errors.CompartmentStockCannotBeZeroWhenItemPairingIsNotFixed);
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
            if (source.XPosition >= target.XPosition
                && source.XPosition < targetXPositionFinal
                && source.YPosition >= target.YPosition
                && source.YPosition < targetYPositionFinal)
            {
                return true;
            }

            // Bottom Right
            if (sourceXPositionFinal > target.XPosition
                && sourceXPositionFinal <= targetXPositionFinal
                && source.YPosition >= target.YPosition
                && source.YPosition < targetYPositionFinal)
            {
                return true;
            }

            // Top Left
            if (source.XPosition >= target.XPosition
                && source.XPosition < targetXPositionFinal
                && sourceYPositionFinal > target.YPosition
                && sourceYPositionFinal <= targetYPositionFinal)
            {
                return true;
            }

            // Top Right
            if (sourceXPositionFinal > target.XPosition
                && sourceXPositionFinal <= targetXPositionFinal
                && sourceYPositionFinal > target.YPosition
                && sourceYPositionFinal <= targetYPositionFinal)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
