using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public sealed class ItemAvailable : BaseModel<int>
    {
        #region Properties

        public DateTime? LastPickDate { get; set; }

        public ItemManagementType ManagementType { get; set; }

        #endregion
    }
}
