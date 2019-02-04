using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IUpdateController<T>
    {
        #region Methods

        Task<ActionResult<T>> UpdateAsync(T model);

        #endregion
    }
}
