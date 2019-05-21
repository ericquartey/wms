using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CompartmentSetForPut
    {
        #region Properties

        public DateTime? FifoStartDate { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public double RemainingCapacity { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion
    }
}
