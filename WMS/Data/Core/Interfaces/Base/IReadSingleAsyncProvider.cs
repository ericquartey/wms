using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IReadSingleAsyncProvider<TModel, in TKey>
        where TModel : BaseModel<TKey>
    {
        #region Methods

        Task<TModel> GetByIdAsync(TKey id);

        #endregion
    }
}
