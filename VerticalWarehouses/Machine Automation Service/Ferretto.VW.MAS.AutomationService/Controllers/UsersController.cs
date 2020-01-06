using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserAccessLevel = Ferretto.VW.CommonUtils.Messages.Enumerations.UserAccessLevel;
using UserClaims = Ferretto.VW.CommonUtils.Messages.Data.UserClaims;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseWmsProxyController
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly ILogger<UsersController> logger;

        private readonly IUsersProvider usersProvider;

        private readonly IUsersWmsWebService usersWmsWebService;

        #endregion

        #region Constructors

        public UsersController(
            IUsersWmsWebService usersWmsWebService,
            IUsersProvider usersProvider,
            IConfiguration configuration,
            ILogger<UsersController> logger)
        {
            this.usersWmsWebService = usersWmsWebService ?? throw new ArgumentNullException(nameof(usersWmsWebService));
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
            string password,
            string supportToken)
        {
            this.logger.LogDebug($"Login requested for user '{userName}'.");

            if (string.IsNullOrEmpty(supportToken) && this.configuration.IsWmsEnabled())
            {
                try
                {
                    var claims = await this.usersWmsWebService
                        .AuthenticateWithResourceOwnerPasswordAsync(userName, password);

                    this.logger.LogInformation($"Login success for user '{userName}' through WMS.");

                    return this.Ok(claims);
                }
                catch
                {
                    this.logger.LogWarning($"Unable to authenticate user '{userName}' through WMS.");
                }
            }

            var accessLevel = this.usersProvider.Authenticate(userName, password, supportToken);
            if (!accessLevel.HasValue)
            {
                this.logger.LogWarning($"Login for '{userName}' failed.");
                return this.Unauthorized();
            }

            this.logger.LogInformation($"Login success for user '{userName}' using local credentials.");

            return this.Ok(
                new UserClaims
                {
                    Name = userName,
                    AccessLevel = (UserAccessLevel)accessLevel.Value,
                });
        }

        [HttpPost("token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<string>> GetSupportToken()
        {
            var token = this.usersProvider.GetSupportToken();

            this.logger.LogInformation($"Get token: {token}");

            return this.Ok(token);
        }

        #endregion
    }
}
