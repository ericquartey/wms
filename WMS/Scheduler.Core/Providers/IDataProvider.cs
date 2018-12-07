using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface IDataProvider
    {
        #region Methods

        void Add(SchedulerRequest model);

        void AddRange(IEnumerable<Mission> missions);

        Task<Area> GetAreaByIdAsync(int areaId);

        Task<Bay> GetBayByIdAsync(int bayId);

        Task<Item> GetItemByIdAsync(int itemId);

        Task<ItemList> GetListByIdAsync(int listId);

        Task<IEnumerable<SchedulerRequest>> GetRequestsToProcessAsync();

        void Update(Compartment compartment);

        void Update(SchedulerRequest request);

        #endregion Methods
    }
}
