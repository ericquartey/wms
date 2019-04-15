using System;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public class SchedulerRequest : BaseModel<int>
    {
        #region Properties

        public string AreaDescription { get; set; }

        public string BayDescription { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsInstant { get; set; }

        public string ItemDescription { get; set; }

        public string ItemUnitMeasure { get; set; }

        public DateTime LastModificationDate { get; set; }

        public string ListDescription { get; set; }

        public string ListRowDescription { get; set; }

        public string LoadingUnitDescription { get; set; }

        public string LoadingUnitTypeDescription { get; set; }

        public string Lot { get; set; }

        public string MaterialStatusDescription { get; set; }

        public OperationType OperationType { get; set; }

        public string PackageTypeDescription { get; set; }

        public string RegistrationNumber { get; set; }

        public double? RequestedQuantity { get; set; }

        public double? ReservedQuantity { get; set; }

        public SchedulerRequestStatus Status { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public SchedulerRequestType Type { get; set; }

        #endregion
    }
}
