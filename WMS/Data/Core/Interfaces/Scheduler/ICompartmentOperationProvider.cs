using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICompartmentOperationProvider :
        IUpdateAsyncProvider<CandidateCompartment, int>
    {
        #region Methods

        Task<CandidateCompartment> GetByIdForStockUpdateAsync(int id);

        IQueryable<CandidateCompartment> GetCandidateCompartments(ItemSchedulerRequest request);

        Expression<Func<Common.DataModels.Compartment, bool>> GetCompartmentIsInBayFunction(
                            int? bayId,
            bool isVertimag = true);

        IQueryable<T> OrderCompartmentsByManagementType<T>(
            IQueryable<T> compartments,
            ItemManagementType managementType,
            OperationType operationType)
                where T : IOrderableCompartment;

        #endregion
    }
}
