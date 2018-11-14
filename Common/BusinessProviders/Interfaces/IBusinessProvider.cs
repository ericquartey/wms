using System.Linq;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.BusinessProviders
{
    public interface IBusinessProvider<out TModel, TDetailsModel>
        where TModel : IBusinessObject
        where TDetailsModel : IBusinessObject
    {
        #region Methods

        int Add(TDetailsModel model);

        int Delete(int id);

        IQueryable<TModel> GetAll();

        int GetAllCount();

        TDetailsModel GetById(int id);

        int Save(TDetailsModel model);

        #endregion Methods
    }
}
