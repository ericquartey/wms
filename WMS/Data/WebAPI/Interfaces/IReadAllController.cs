using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IReadAllController<T>
    {
        #region Methods

        ActionResult<IEnumerable<T>> GetAll();

        #endregion Methods
    }
}
