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
          string whereExpression = null,
          IEnumerable<SortOption> orderBy = null,
          string searchExpression = null);

        Task<int> GetAllCountAsync(
          string whereExpression = null,
          string searchExpression = null);

        Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName);

        #endregion
    }
}
