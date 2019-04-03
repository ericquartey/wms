using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IBaySchedulerProvider
    {
        #region Methods

        Task UpdatePriorityAsync(int id);

        #endregion
    }
}
