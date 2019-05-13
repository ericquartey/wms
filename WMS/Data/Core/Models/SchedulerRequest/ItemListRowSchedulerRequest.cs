using System;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListRowSchedulerRequest : ItemSchedulerRequest
    {
        #region Properties

        public int ListId { get; set; }

        public int ListRowId { get; set; }

        public override SchedulerRequestType Type { get => SchedulerRequestType.ItemListRow; }

        #endregion
    }
}
