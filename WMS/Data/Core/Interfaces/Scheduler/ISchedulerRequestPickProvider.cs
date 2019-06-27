using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ISchedulerRequestPickProvider
    {
        #region Methods

        Task<IOperationResult<IEnumerable<ItemSchedulerRequest>>> FullyQualifyPickRequestAsync(
            int itemId,
            ItemOptions itemOptions,
            ItemListRowOperation row = null,
            int? previousRowRequestPriority = null);

        Task<IOperationResult<double>> GetItemAvailabilityAsync(int itemId, ItemOptions itemPickOptions);

        #endregion
    }
}
