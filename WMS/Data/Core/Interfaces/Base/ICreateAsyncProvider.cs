using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface ICreateAsyncProvider<TModel, TKey>
        where TModel : BaseModel<TKey>
    {
        #region Methods

        Task<OperationResult<TModel>> CreateAsync(TModel model);

        #endregion
    }
}
