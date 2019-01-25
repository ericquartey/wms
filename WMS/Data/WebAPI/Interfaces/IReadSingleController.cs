using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IReadSingleController<T>
    {
        #region Methods

        ActionResult<T> GetById(int id);

        #endregion Methods
    }
}
