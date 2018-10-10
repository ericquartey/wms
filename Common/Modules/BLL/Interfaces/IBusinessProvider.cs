using System.Linq;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Modules.BLL
{
    public interface IBusinessProvider<out TModel, TDetailsModel, TId>
        where TModel : IBusinessObject<TId>
        where TDetailsModel : IBusinessObject<TId>
    {
        #region Methods

        IQueryable<TModel> GetAll();

        int GetAllCount();

        TDetailsModel GetById(TId id);

        int Save(TDetailsModel model);

        #endregion Methods
    }
}
