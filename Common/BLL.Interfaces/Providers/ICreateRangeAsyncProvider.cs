using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface ICreateRangeAsyncProvider<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<IOperationResult<IEnumerable<TModel>>> CreateRangeAsync(IEnumerable<TModel> models);

        #endregion
    }
}
