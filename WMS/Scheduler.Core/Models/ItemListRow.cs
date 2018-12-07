namespace Ferretto.WMS.Scheduler.Core
{
    public class ItemListRow : BusinessObject
    {
        #region Properties

        public int ItemId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public int RequestedQuantity { get; set; }

        public ListRowStatus Status { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion Properties
    }
}
