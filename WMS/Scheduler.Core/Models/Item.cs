using Ferretto.Common.BusinessModels;

namespace Ferretto.WMS.Scheduler.Core
{
    public sealed class Item : BusinessObject
    {
        #region Properties

        public ItemManagementType ManagementType { get; set; }

        #endregion Properties
    }
}
