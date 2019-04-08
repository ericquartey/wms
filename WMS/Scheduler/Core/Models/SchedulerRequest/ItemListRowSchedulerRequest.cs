using System;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class ItemListRowSchedulerRequest : ItemSchedulerRequest
    {
        #region Properties

        public int ListId { get; set; }

        public int ListRowId { get; set; }

        public override SchedulerType SchedulerType { get => SchedulerType.ItemListRow; }

        #endregion
    }
}
