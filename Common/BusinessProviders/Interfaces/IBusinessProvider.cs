using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;

namespace Ferretto.Common.BusinessProviders
{
    public interface IBusinessProvider<out TModel, TDetailsModel, TKey>
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
