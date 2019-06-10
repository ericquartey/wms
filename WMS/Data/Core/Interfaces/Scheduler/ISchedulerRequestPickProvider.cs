using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ISchedulerRequestPickProvider
    {
        #region Methods

        Task<IOperationResult<ItemSchedulerRequest>> FullyQualifyPickRequestAsync(
            int itemId,
            ItemOptions itemPickOptions,
            ItemListRowOperation row = null,
            int? previousRowRequestPriority = null);

        Task<IOperationResult<double>> GetItemAvailabilityAsync(int itemId, ItemOptions itemPickOptions);

        #endregion
    }
}
