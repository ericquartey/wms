using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("identity")]
    [Authorize]
    public class IdentityController : ControllerBase
    {
        #region Methods

        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(from c in this.User.Claims select new { c.Type, c.Value });
        }

        #endregion
    }
}
