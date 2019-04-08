using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class MissionDetails : BaseModel<int>
    {
        #region Properties

        public int? BayId { get; set; }

        public int? CompartmentId { get; set; }

        public DateTime CreationDate { get; set; }

        public int? ItemId { get; set; }

        public ItemList ItemList { get; set; }

        public ItemListRow ItemListRow { get; set; }

        public LoadingUnitContentInfo LoadingUnit { get; set; }

        public string Lot { get; set; }

        public string MaterialStatusDescription { get; set; }

        public int? MaterialStatusId { get; set; }

        public string PackageTypeDescription { get; set; }

        public int? PackageTypeId { get; set; }

        public int Priority { get; set; }

        public string RegistrationNumber { get; set; }

        public int RequestedQuantity { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public MissionType Type { get; set; }

        #endregion
    }
}
