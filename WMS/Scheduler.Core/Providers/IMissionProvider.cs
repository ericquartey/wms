using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface IMissionProvider
    {
        #region Methods

        Task<int> AddRange(IEnumerable<Mission> missions);

        Task<Item> GetItemByIdAsync(int itemId);

        #endregion Methods
    }
}
