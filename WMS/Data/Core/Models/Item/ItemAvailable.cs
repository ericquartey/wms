using System;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Item))]
    public sealed class ItemAvailable : BaseModel<int>
    {
        #region Properties

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastPutDate { get; set; }

        public ItemManagementType ManagementType { get; set; }

        #endregion
    }
}
