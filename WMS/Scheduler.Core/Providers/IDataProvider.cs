using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface IDataProvider
    {
        #region Methods

        void Add(SchedulerRequest model);

        void AddRange(IEnumerable<Mission> missions);


        Task<int> AddRangeAsync(IEnumerable<Mission> missions);

        Task<Item> GetItemByIdAsync(int itemId);

        Task<SchedulerRequest> GetNextRequestToProcessAsync();

        void Update(Compartment compartment);

        void Update(SchedulerRequest request);

        #endregion Methods
    }
}
