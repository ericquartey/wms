using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class StockUpdateCompartment : BaseModel<int>
    {
        #region Properties

        public bool IsItemPairingFixed { get; set; }

        public int? ItemId { get; set; }

        public DateTime? LastPickDate { get; set; }

        public int LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public double ReservedForPick { get; set; }

        public double Stock { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion
    }
}
