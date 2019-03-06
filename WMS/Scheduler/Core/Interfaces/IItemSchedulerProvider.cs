using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemSchedulerProvider
    {
        #region Methods

        Task<Item> GetByIdAsync(int itemId);

        #endregion
    }
}
