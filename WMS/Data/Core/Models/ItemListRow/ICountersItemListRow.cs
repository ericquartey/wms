using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface ICountersItemListRow : IModel<int>
    {
        #region Properties

        int SchedulerRequestsCount { get; set; }

        #endregion
    }
}
