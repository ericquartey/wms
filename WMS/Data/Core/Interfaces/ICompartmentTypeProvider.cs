using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICompartmentTypeProvider :
        ICreateAsyncProvider<CompartmentType>,
        IReadAllAsyncProvider<CompartmentType>,
        IReadSingleAsyncProvider<CompartmentType, int>
    {
        #region Methods

        Task<OperationResult<CompartmentType>> CreateAsync(
            CompartmentType model,
            int? itemId,
            int? maxCapacity);

        #endregion
    }
}
