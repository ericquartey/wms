using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ISchedulerRequestPutProvider
    {
        #region Methods

        Task<ItemSchedulerRequest> FullyQualifyPutRequestAsync(
                                          int itemId,
                                          ItemOptions itemPutOptions,
                                          ItemListRowOperation row = null,
                                          int? previousRowRequestPriority = null);

        #endregion
    }
}
