using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface ICompartmentTypeProvider :
        IPagedBusinessProvider<CompartmentType, int>,
        IReadAllAsyncProvider<Enumeration, int>,
        IReadSingleAsyncProvider<CompartmentType, int>,
        IDeleteAsyncProvider<CompartmentType, int>
    {
        #region Methods

        Task<IOperationResult<CompartmentType>> CreateAsync(CompartmentType model, int? itemId = null, int? maxCapacity = null);

        #endregion
    }
}
