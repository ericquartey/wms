using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IPagedBusinessProvider<TModel>
        where TModel : IBusinessObject
    {
        #region Methods

        Task<IEnumerable<TModel>> GetAllAsync(
          int take = 0,
          int skip = 0,
          string where = null,
          IEnumerable<SortOption> orderBy = null,
          string search = null);

        Task<int> GetAllCountAsync(
          string where = null,
          string search = null);

        #endregion Methods
    }
}
