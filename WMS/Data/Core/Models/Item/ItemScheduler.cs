using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public sealed class ItemScheduler : Model<int>
    {
        #region Properties

        public DateTime? LastPickDate { get; set; }

        public ItemManagementType ManagementType { get; set; }

        #endregion
    }
}
