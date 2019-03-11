using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IReadAllController<T>
    {
        #region Methods

        Task<ActionResult<IEnumerable<T>>> GetAllAsync();

        Task<ActionResult<int>> GetAllCountAsync();

        #endregion
    }
}
