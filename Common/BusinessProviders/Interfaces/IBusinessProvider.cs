using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IBusinessProvider<out TModel, TDetailsModel>
        where TModel : IBusinessObject
        where TDetailsModel : IBusinessObject
    {
        #region Methods

        Task<IOperationResult> AddAsync(TDetailsModel model);

        Task<int> DeleteAsync(int id);

        IQueryable<TModel> GetAll();

        int GetAllCount();

        Task<TDetailsModel> GetByIdAsync(int id);

        TDetailsModel GetNew();

        Task<IOperationResult> SaveAsync(TDetailsModel model);

        #endregion
    }
}
