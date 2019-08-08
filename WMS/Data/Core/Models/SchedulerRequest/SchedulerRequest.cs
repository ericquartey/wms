using System;
using Ferretto.Common.Utils;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(SchedulerRequest))]
    public class SchedulerRequest : BaseModel<int>
    {
        #region Fields

        public const int InstantRequestPriority = 1;

        #endregion

        #region Properties

        public string AreaName { get; set; }

        public string BayDescription { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsInstant { get; set; }

        public string ItemDescription { get; set; }

        public DateTime LastModificationDate { get; set; }

        public string ListDescription { get; set; }

        public string ListRowCode { get; set; }

        public string LoadingUnitCode { get; set; }

        public string Lot { get; set; }

        public string MaterialStatusDescription { get; set; }

        public string MeasureUnitDescription { get; set; }

        public Enums.OperationType OperationType { get; set; }

        public string PackageTypeDescription { get; set; }

        public string RegistrationNumber { get; set; }

        public double? RequestedQuantity { get; set; }

        public double? ReservedQuantity { get; set; }

        public Enums.SchedulerRequestStatus Status { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public Enums.SchedulerRequestType Type { get; set; }

        #endregion
    }
}
