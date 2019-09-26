using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseWmsProxyBaseController
    {
        #region Fields

        private readonly ILogger<UsersController> logger;

        private readonly IUsersDataService usersDataService;

        private readonly IUsersProvider usersProvider;

        #endregion

        #region Constructors

        public UsersController(
            IUsersDataService usersDataService,
            IUsersProvider usersProvider,
            ILogger<UsersController> logger)
        {
            if (usersDataService is null)
            {
                throw new ArgumentNullException(nameof(usersDataService));
            }

            if (usersProvider is null)
            {
                throw new ArgumentNullException(nameof(usersProvider));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.usersDataService = usersDataService;
            this.usersProvider = usersProvider;
            this.logger = logger;
        }

        #endregion

        #region Methods

        [HttpPost("authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<UserClaims>> AuthenticateWithResourceOwnerPassword(
            string userName,
            string password)
        {
            try
            {
                this.logger.LogInformation($"Login requested for user '{userName}'. Forwarding authentication request to WMS ...");
                return this.Ok(await this.usersDataService
                    .AuthenticateWithResourceOwnerPasswordAsync(userName, password));
            }
            catch
            {
                this.logger.LogWarning($"Unable to authenticate user '{userName}' through WMS. Using local credentials.");

                var accessLevel = this.usersProvider.Authenticate(userName, password);
                if (!accessLevel.HasValue)
                {
                    this.logger.LogWarning($"Login for '{userName}' failed.");
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
