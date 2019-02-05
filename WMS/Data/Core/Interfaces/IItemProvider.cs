using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemProvider : IGetUniqueValuesProvider
    {
        #region Methods

        Task<IEnumerable<Item>> GetAllAsync();

        Task<IEnumerable<Item>> GetAllAsync(
            int skip,
            int take,
            string orderBy = null,
            Expression<Func<Item, bool>> whereExpression = null,
            Expression<Func<Item, bool>> searchExpression = null);

        Task<int> GetAllCountAsync(
            Expression<Func<Item, bool>> whereExpression = null,
            Expression<Func<Item, bool>> searchExpression = null);

        Task<Item> GetByIdAsync(int id);

        Task<Item> UpdateAsync(Item item);

        #endregion
    }
}
