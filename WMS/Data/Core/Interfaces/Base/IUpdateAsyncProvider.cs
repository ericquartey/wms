using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IUpdateAsyncProvider<TModel, TKey>
        where TModel : BaseModel<TKey>
    {
        #region Methods

        Task<OperationResult<TModel>> UpdateAsync(TModel model);

        #endregion
    }
}
