using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IUpdateController<in T>
    {
        #region Methods

        Task<ActionResult> UpdateAsync(T model);

        #endregion Methods
    }
}
