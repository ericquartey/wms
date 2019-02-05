using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IDeleteController<in T>
    {
        #region Methods

        Task<ActionResult> DeleteAsync(T model);

        #endregion
    }
}
