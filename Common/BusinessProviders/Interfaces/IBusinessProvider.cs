using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BusinessProviders
{
#pragma warning disable S2436 // Types and methods should not have too many generic parameters - this interface is soon going to be dismissed

    public interface IBusinessProvider<out TModel, TDetailsModel, TKey>
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
        where TModel : IModel<TKey>
        where TDetailsModel : IModel<TKey>
    {
        #region Methods

        Task<IOperationResult<TDetailsModel>> AddAsync(TDetailsModel model);

        Task<int> DeleteAsync(int id);

        IQueryable<TModel> GetAll();

        int GetAllCount();

        Task<TDetailsModel> GetByIdAsync(int id);

        TDetailsModel GetNew();

        Task<IOperationResult<TDetailsModel>> SaveAsync(TDetailsModel model);

        #endregion
    }
}
