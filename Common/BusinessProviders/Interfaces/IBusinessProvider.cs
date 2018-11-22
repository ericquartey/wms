using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.BusinessProviders
{
    public interface IBusinessProvider<out TModel, TDetailsModel>
        where TModel : IBusinessObject
        where TDetailsModel : IBusinessObject
    {
        #region Methods

        Task<int> Add(TDetailsModel model);

        int Delete(int id);

        IQueryable<TModel> GetAll();

        int GetAllCount();

        TDetailsModel GetById(int id);

        int Save(TDetailsModel model);

        #endregion Methods
    }
}
