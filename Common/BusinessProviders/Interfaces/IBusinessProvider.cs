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

        Task<OperationResult> AddAsync(TDetailsModel model);

        Task<int> DeleteAsync(int id);

        IQueryable<TModel> GetAll();

        int GetAllCount();

        Task<TDetailsModel> GetByIdAsync(int id);

        Task<OperationResult> SaveAsync(TDetailsModel model);

        #endregion Methods
    }
}
