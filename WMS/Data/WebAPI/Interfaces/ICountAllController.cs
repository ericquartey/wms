using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface ICountAllController
    {
        #region Methods

        Task<ActionResult<int>> GetAllCountAsync();

        #endregion
    }
}
