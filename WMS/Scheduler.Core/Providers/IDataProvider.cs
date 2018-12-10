using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface IDataProvider
    {
        #region Methods

        bool Add(SchedulerRequest model);

        void AddRange(IEnumerable<Mission> missions);

        void AddRange(IEnumerable<SchedulerRequest> requests);

        Task<Area> GetAreaByIdAsync(int areaId);

        Task<Bay> GetBayByIdAsync(int bayId);

        Task<Item> GetItemByIdAsync(int itemId);

        Task<ItemList> GetListByIdAsync(int listId);

        Task<IEnumerable<SchedulerRequest>> GetRequestsToProcessAsync();

        void Update(Compartment compartment);

        void Update(SchedulerRequest request);

        void Update(ItemList list);

        void Update(ItemListRow row);

        #endregion Methods
    }
}
