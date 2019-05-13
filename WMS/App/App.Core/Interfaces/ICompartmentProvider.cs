using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.Controls.WPF;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface ICompartmentProvider :
        IPagedBusinessProvider<Compartment, int>,
        IReadSingleAsyncProvider<CompartmentDetails, int>,
        ICreateAsyncProvider<CompartmentDetails, int>,
        IUpdateAsyncProvider<CompartmentDetails, int>,
        IDeleteAsyncProvider<CompartmentDetails, int>
    {
        #region Methods

        Task<IOperationResult<IDrawableCompartment>> AddRangeAsync(IEnumerable<IDrawableCompartment> compartments);

        Task<IEnumerable<Compartment>> GetByItemIdAsync(int id);

        Task<IOperationResult<IEnumerable<CompartmentDetails>>> GetByLoadingUnitIdAsync(int id);

        Task<double?> GetMaxCapacityAsync(double? width, double? height, int itemId);

        Task<CompartmentDetails> GetNewAsync();

        #endregion
    }
}
