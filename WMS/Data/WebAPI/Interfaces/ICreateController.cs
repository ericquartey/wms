using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface ICreateController<T>
    {
        #region Methods

        Task<ActionResult<T>> CreateAsync(T model);

        #endregion Methods
    }
}
