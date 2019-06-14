using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(SchedulerRequest))]
    public class ItemListRowSchedulerRequest : ItemSchedulerRequest
    {
        #region Properties

        public int ListId { get; set; }

        public int ListRowId { get; set; }

        public override SchedulerRequestType Type { get => SchedulerRequestType.ItemListRow; }

        #endregion
    }
}
