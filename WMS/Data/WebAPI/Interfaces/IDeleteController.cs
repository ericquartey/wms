using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IDeleteController<in TKey>
    {
        #region Methods

        Task<ActionResult> DeleteAsync(TKey id);

        #endregion
    }
}
