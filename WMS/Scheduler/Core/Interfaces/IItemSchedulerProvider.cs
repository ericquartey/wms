using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemSchedulerProvider
    {
        #region Methods

        Task<Item> GetByIdAsync(int itemId);

        #endregion
    }
}
