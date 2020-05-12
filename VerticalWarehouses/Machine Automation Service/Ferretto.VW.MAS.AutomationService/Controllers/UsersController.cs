using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserAccessLevel = Ferretto.VW.CommonUtils.Messages.Enumerations.UserAccessLevel;
using UserClaims = Ferretto.VW.CommonUtils.Messages.Data.UserClaims;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseWmsProxyController, IRequestingBayController
    {
        #region Fields

        private readonly ILogger<UsersController> logger;

        private readonly IUsersProvider usersProvider;

        #endregion

        #region Constructors

        public UsersController(
            IUsersProvider usersProvider,
            ILogger<UsersController> logger)
        {
            this.usersProvider = usersProvider ?? throw new ArgumentNullException(nameof(usersProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("authenticate-bearer-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<UserClaims>> AuthenticateWithBearerToken(
           string bearerToken,
           [FromServices] IWmsSettingsProvider wmsSettingsProvider,
           [FromServices] IUsersWmsWebService usersWmsWebService)
        {
            this.logger.LogDebug($"Login requested for token '{bearerToken}' by '{this.BayNumber}'.");

            if (wmsSettingsProvider.IsEnabled)
            {
                var claims = await usersWmsWebService.AuthenticateWithBearerTokenAsync(bearerToken);

                this.logger.LogInformation($"Login success for user '{claims.Name}' by '{this.BayNumber}' through WMS.");

                return this.Ok(claims);
            }

            return this.BadRequest("The Wms is not enabled.");
        }

        [HttpPost("authenticate-resource-owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<UserClaims>> AuthenticateWithResourceOwnerPassword(
            string userName,
            string password,
            string supportToken,
            [FromServices] IWmsSettingsProvider wmsSettingsProvider,
            [FromServices] IUsersWmsWebService usersWmsWebService)
        {
            this.logger.LogDebug($"Login requested for user '{userName}' by '{this.BayNumber}'.");

            if (string.IsNullOrEmpty(supportToken) && wmsSettingsProvider.IsEnabled)
            {
                try
                {
                    var claims = await usersWmsWebService.AuthenticateWithResourceOwnerPasswordAsync(userName, password);

                    this.logger.LogInformation($"Login success for user '{userName}' by '{this.BayNumber}' through WMS.");

                    return this.Ok(claims);
                }
                catch
                {
                    this.logger.LogWarning($"Unable to authenticate user '{userName}' by '{this.BayNumber}' through WMS.");
                }
            }

            var accessLevel = this.usersProvider.Authenticate(userName, password, supportToken);
            if (!accessLevel.HasValue)
            {
                this.logger.LogWarning($"Login for '{userName}' by '{this.BayNumber}' failed.");
                return this.Unauthorized();
            }

            this.logger.LogInformation($"Login success for user '{userName}' by '{this.BayNumber}' using local credentials.");

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
        public ActionResult<string> GetSupportToken()
        {
            var token = this.usersProvider.GetServiceToken();

            this.logger.LogInformation($"Get token: {token}");

            return this.Ok(token);
        }

        #endregion
    }
}
