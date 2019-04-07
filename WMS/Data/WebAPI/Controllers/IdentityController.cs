using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class IdentityController : ControllerBase
    {
        #region Methods

        [HttpGet]
        public IActionResult Get()
        {
            var claims = this.User.Claims.Select(c => new { c.Type, c.Value });

            return this.Ok(claims);
        }

        #endregion
    }
}
