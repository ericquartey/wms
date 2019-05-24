using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICompartmentOperationProvider :
        IUpdateAsyncProvider<StockUpdateCompartment, int>,
        IUpdateAsyncProvider<CandidateCompartment, int>
    {
        #region Methods

        Task<StockUpdateCompartment> GetByIdForStockUpdateAsync(int id);

        IQueryable<CandidateCompartment> GetCandidateCompartments(ItemSchedulerRequest request);

        IQueryable<T> OrderPickCompartmentsByManagementType<T>(IQueryable<T> compartments, ItemManagementType type)
                    where T : IOrderableCompartment;

        IQueryable<T> OrderPutCompartmentsByManagementType<T>(IQueryable<T> compartments, ItemManagementType type)
                    where T : IOrderableCompartment;

        #endregion
    }
}
