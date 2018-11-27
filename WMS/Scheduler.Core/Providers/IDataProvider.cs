using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface IDataProvider
    {
        #region Methods

        Task<int> AddAsync(SchedulerRequest model);

        Task<int> AddRangeAsync(IEnumerable<Mission> missions);

        Task<Item> GetItemByIdAsync(int itemId);

        Task<SchedulerRequest> GetNextRequestToProcessAsync();

        #endregion Methods
    }
}
