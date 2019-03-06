using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IAreaSchedulerProvider
    {
        #region Methods

        Task<Area> GetByIdAsync(int id);

        #endregion
    }
}
