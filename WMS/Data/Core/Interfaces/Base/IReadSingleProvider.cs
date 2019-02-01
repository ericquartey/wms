using System.Threading.Tasks;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IReadSingleProvider<TModel, in TKey>
    {
        #region Methods

        Task<TModel> GetByIdAsync(TKey id);

        #endregion Methods
    }
}
