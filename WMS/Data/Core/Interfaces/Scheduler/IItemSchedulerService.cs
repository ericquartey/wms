using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemSchedulerService
    {
        #region Methods

        Task<IOperationResult<double>> GetPickAvailabilityAsync(int itemId, ItemOptions options);

        Task<IOperationResult<double>> GetPutCapacityAsync(int itemId, ItemOptions options);

        Task<IOperationResult<IEnumerable<ItemSchedulerRequest>>> PickItemAsync(int itemId, ItemOptions options);

        Task<IOperationResult<IEnumerable<ItemSchedulerRequest>>> PutItemAsync(int itemId, ItemOptions options);

        #endregion
    }
}
