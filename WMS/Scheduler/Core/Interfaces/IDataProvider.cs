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

        void Update(Compartment compartment);

        void Update(SchedulerRequest request);

        #endregion
    }
}
