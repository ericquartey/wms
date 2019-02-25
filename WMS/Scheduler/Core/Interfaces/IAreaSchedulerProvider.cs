using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IAreaSchedulerProvider
    {
        #region Methods

        Task<Area> GetByIdAsync(int id);

        #endregion
    }
}
