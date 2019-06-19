using System;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CandidateCompartment : BaseModel<int>, IOrderableCompartment
    {
        #region Properties

        public int AreaId { get; set; }

        public double Availability => this.Stock - this.ReservedForPick + this.ReservedToPut;

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

        [Positive]
        public double? MaxCapacity { get; set; }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public double RemainingCapacity => this.MaxCapacity.HasValue ? this.MaxCapacity.Value - this.Availability : double.PositiveInfinity;

        [PositiveOrZero]
        public double ReservedForPick { get; set; }

        [PositiveOrZero]
        public double ReservedToPut { get; set; }

        [PositiveOrZero]
        public double Stock { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion
    }
}
