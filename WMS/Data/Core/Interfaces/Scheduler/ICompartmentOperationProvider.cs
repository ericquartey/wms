using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICompartmentOperationProvider :
        IUpdateAsyncProvider<StockUpdateCompartment, int>,
        IUpdateAsyncProvider<CompartmentWithdraw, int>
    {
        #region Methods

        Task<StockUpdateCompartment> GetByIdForStockUpdateAsync(int id);

        IQueryable<CompartmentWithdraw> GetCandidatePickCompartments(ItemSchedulerRequest schedulerRequest);

        IQueryable<T> OrderPickCompartmentsByManagementType<T>(IQueryable<T> compartments, ItemManagementType type)
            where T : IOrderableCompartment;

        #endregion
    }
}
