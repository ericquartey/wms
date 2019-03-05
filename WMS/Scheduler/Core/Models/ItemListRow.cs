using System;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class ItemListRow : Model
    {
        #region Properties

        public string Code { get; set; }

        public DateTime CreationDate { get; set; }

        public string ItemDescription { get; set; }

        public int ItemId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public int RequestedQuantity { get; set; }

        public int RequiredQuantity { get; set; }

        public int RowPriority { get; set; }

        public ListRowStatus Status { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion
    }
}
