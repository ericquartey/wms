using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ILoadingUnitSchedulerProvider
    {
        #region Methods

        Task<LoadingUnit> GetByIdAsync(int id);

        #endregion
    }
}
