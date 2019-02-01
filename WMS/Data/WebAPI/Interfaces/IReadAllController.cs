using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IReadAllController<T>
    {
        #region Methods

        Task<ActionResult<IEnumerable<T>>> GetAll();

        #endregion Methods
    }
}
