using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : AutomationBaseController
    {
        #region Fields

        private readonly IUsersDataService usersDataService;

        private readonly IUsersProvider usersProvider;

        #endregion

        #region Constructors

        public UsersController(
            IUsersDataService usersDataService,
            IUsersProvider usersProvider)
        {
            if (usersDataService == null)
            {
                throw new ArgumentNullException(nameof(usersDataService));
            }

            if (usersProvider == null)
            {
                throw new ArgumentNullException(nameof(usersProvider));
            }

            this.usersDataService = usersDataService;
            this.usersProvider = usersProvider;
        }

        #endregion

        #region Methods

        [HttpPost("authenticate")]
        public async Task<ActionResult<UserClaims>> AuthenticateWithResourceOwnerPassword(
            [FromQuery] string userName,
            [FromQuery] string password)
        {
            try
            {
                return this.Ok(await this.usersDataService
                    .AuthenticateWithResourceOwnerPasswordAsync(userName, password));
            }
            catch
            {
                var accessLevel = this.usersProvider.Authenticate(userName, password);
                if (!accessLevel.HasValue)
                {
                    return this.Unauthorized();
                }

                return this.Ok(new UserClaims
                {
                    Name = userName,
                    AccessLevel = (UserAccessLevel)accessLevel.Value
                });
            }
        }

        #endregion
    }
}
