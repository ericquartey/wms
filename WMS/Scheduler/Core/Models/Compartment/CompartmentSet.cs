using System;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class CompartmentSet : IOrderableCompartment
    {
        #region Properties

        public int Availability { get; set; }

        public DateTime? FirstStoreDate { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion
    }
}
