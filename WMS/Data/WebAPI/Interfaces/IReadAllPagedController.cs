using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IReadAllPagedController<T>
    {
        #region Methods

        Task<ActionResult<IEnumerable<Item>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null);

        #endregion
    }
}
