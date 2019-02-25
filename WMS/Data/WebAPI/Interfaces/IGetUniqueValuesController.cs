using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IGetUniqueValuesController
    {
        #region Methods

        Task<ActionResult<object[]>> GetUniqueValuesAsync(string propertyName);

        #endregion
    }
}
