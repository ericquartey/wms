namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemOptions
    {
        #region Properties

        public int AreaId { get; set; }

        public int? BayId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public double Quantity { get; set; }

        public string RegistrationNumber { get; set; }

        public double RequestedQuantity { get; set; }

        public bool RunImmediately { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion
    }
}
