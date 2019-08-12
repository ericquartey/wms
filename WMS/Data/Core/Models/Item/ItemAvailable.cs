using System;
using Ferretto.Common.Utils;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Item))]
    public sealed class ItemAvailable : BaseModel<int>
    {
        #region Properties

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastPutDate { get; set; }

        public Enums.ItemManagementType ManagementType { get; set; }

        #endregion
    }
}
