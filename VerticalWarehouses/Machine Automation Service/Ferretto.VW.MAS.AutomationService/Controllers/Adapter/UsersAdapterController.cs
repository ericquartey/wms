using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersAdapterController : ControllerBase
    {
        #region Constructors

        public UsersAdapterController()
        {
        }

        #endregion

        #region Methods

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [HttpPost("authenticate-token")]
        public ActionResult<CommonUtils.Messages.Data.UserClaims> AuthenticateWithBearerToken(string bearerToken)
        {
            if (bearerToken == null)
            {
                return this.BadRequest(new Microsoft.AspNetCore.Mvc.ProblemDetails { Title = "The bearer token was not provided." });
            }

            throw new NotImplementedException();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [HttpPost("authenticate")]
        public ActionResult<CommonUtils.Messages.Data.UserClaims> AuthenticateWithResourceOwnerPassword(
           string userName,
           string password)
        {
            if (userName == null)
            {
                return this.BadRequest(new Microsoft.AspNetCore.Mvc.ProblemDetails { Title = "User name was not provided." });
            }

            if (password == null)
            {
                return this.BadRequest(new Microsoft.AspNetCore.Mvc.ProblemDetails { Title = "Password was not provided." });
            }

            throw new NotImplementedException();
        }

        [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
