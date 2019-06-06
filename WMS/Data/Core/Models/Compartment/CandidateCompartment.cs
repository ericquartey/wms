using System;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CandidateCompartment : BaseModel<int>, IOrderableCompartment
    {
        #region Fields

        private double? maxCapacity;

        private double reservedForPick;

        private double reservedToPut;

        private double stock;

        #endregion

        #region Properties

        public int AreaId { get; set; }

        public double Availability => this.stock - this.reservedForPick + this.reservedToPut;

        public int? CellId { get; set; }

        public int CompartmentTypeId { get; set; }

        public DateTime? FifoStartDate { get; set; }

        public bool IsItemPairingFixed { get; set; }

        public int? ItemId { get; set; }

        public DateTime? LastPickDate { get; internal set; }

        public DateTime? LastPutDate { get; set; }

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

        public double RemainingCapacity => this.MaxCapacity.HasValue ? this.maxCapacity.Value - this.Availability : double.PositiveInfinity;

        public double ReservedForPick
        {
            get => this.reservedForPick;
            set => this.reservedForPick = CheckIfPositive(value);
        }

        public double ReservedToPut
        {
            get => this.reservedToPut;
            set => this.reservedToPut = CheckIfPositive(value);
        }

        public double Stock
        {
            get => this.stock;
            set => this.stock = CheckIfPositive(value);
        }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion
    }
}
