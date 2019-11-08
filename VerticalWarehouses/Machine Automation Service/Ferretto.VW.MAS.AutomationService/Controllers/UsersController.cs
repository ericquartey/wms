using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseWmsProxyController
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly ILogger<UsersController> logger;

        private readonly IUsersDataService usersDataService;

        private readonly IUsersProvider usersProvider;

        #endregion

        #region Constructors

        public UsersController(
            IUsersDataService usersDataService,
            IUsersProvider usersProvider,
            IConfiguration configuration,
            ILogger<UsersController> logger)
        {
            this.usersDataService = usersDataService ?? throw new ArgumentNullException(nameof(usersDataService));
            this.usersProvider = usersProvider ?? throw new ArgumentNullException(nameof(usersProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            if (this.configuration.IsWmsEnabled())
            {
                try
                {
                    this.logger.LogInformation($"Login requested for user '{userName}'. Forwarding authentication request to WMS ...");

                    return this.Ok(await this.usersDataService
                        .AuthenticateWithResourceOwnerPasswordAsync(userName, password));
                }
                catch
                {
                    this.logger.LogWarning($"Unable to authenticate user '{userName}' through WMS. Will use local credentials.");
                }
            }

            var accessLevel = this.usersProvider.Authenticate(userName, password);
            if (!accessLevel.HasValue)
            {
                this.logger.LogWarning($"Login for '{userName}' failed.");
                return this.Unauthorized();
            }

            return this.Ok(new UserClaims
            {
                Name = userName,
                AccessLevel = (UserAccessLevel)accessLevel.Value,
            });
        }

        #endregion
    }
}
