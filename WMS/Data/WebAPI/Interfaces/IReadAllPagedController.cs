using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IReadAllPagedController<T>
    {
        #region Methods

        ActionResult<IEnumerable<T>> GetAll(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null);

        #endregion Methods
    }
}
