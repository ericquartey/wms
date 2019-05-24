using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IReadAllPagedController<T>
    {
        #region Methods

        Task<ActionResult<IEnumerable<T>>> GetAllAsync(
            int skip = 0,
            int take = 0,
            string where = null,
            string orderBy = null,
            string search = null);

        Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null);

        #endregion
    }
}
