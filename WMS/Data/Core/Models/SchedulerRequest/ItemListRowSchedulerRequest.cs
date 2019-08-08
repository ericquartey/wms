using Ferretto.Common.Utils;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(SchedulerRequest))]
    public class ItemListRowSchedulerRequest : ItemSchedulerRequest
    {
        #region Properties

        public int ListId { get; set; }

        public int ListRowId { get; set; }

        public override Enums.SchedulerRequestType Type { get => Enums.SchedulerRequestType.ItemListRow; }

        #endregion
    }
}
