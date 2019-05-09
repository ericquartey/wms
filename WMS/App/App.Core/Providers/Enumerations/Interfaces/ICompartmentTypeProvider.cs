using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public interface ICompartmentTypeProvider :
        IReadAllAsyncProvider<Enumeration, int>
    {
        #region Methods

        Task<IOperationResult<CompartmentType>> CreateAsync(CompartmentType model, int? itemId = null, int? maxCapacity = null);

        #endregion
    }
}
