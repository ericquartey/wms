using System;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListRowOperation : BaseModel<int>, IItemListRowExecutePolicy
    {
        #region Properties

        public DateTime? CompletionDate { get; set; }

        public double DispatchedQuantity { get; set; }

        public int ItemId { get; set; }

        public DateTime? LastExecutionDate { get; set; }

        public int ListId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public int? Priority { get; set; }

        public string RegistrationNumber { get; set; }

        public double RequestedQuantity { get; set; }

        public ItemListRowStatus Status { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion
    }
}
