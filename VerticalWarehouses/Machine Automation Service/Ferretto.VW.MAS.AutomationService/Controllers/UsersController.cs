﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ILogger<UsersController> logger;

        private readonly IUsersProvider usersProvider;

        private readonly IUsersWmsWebService usersWmsWebService;

        #endregion

        #region Constructors

        public UsersController(
            IUsersProvider usersProvider,
            IUsersWmsWebService usersWmsWebService,
            IBaysDataProvider baysDataProvider,
            ILogger<UsersController> logger)
        {
            this.usersProvider = usersProvider ?? throw new ArgumentNullException(nameof(usersProvider));
            this.usersWmsWebService = usersWmsWebService ?? throw new ArgumentNullException(nameof(usersWmsWebService));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("add-user")]
        public IActionResult AddUser(Ferretto.VW.MAS.DataModels.UserParameters user)
        {
            this.logger.LogInformation($"Add user '{user.Name}' by '{this.BayNumber}'.");

            this.usersProvider.AddUser(user);
            return this.Ok();
        }

        [HttpPost("authenticate-bearer-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<UserClaims>> AuthenticateWithBearerToken(
           string bearerToken,
           [FromServices] IWmsSettingsProvider wmsSettingsProvider)
        {
            this.logger.LogDebug($"Login requested for token '{bearerToken}' by '{this.BayNumber}'.");

            if (wmsSettingsProvider.IsEnabled)
            {
                var claims = await this.usersWmsWebService.AuthenticateWithBearerTokenAsync(bearerToken);

                this.logger.LogInformation($"Login success for user '{claims.Name}' by '{this.BayNumber}' through WMS.");

                return this.Ok(claims);
            }
            var bay = this.baysDataProvider.GetByNumber(this.BayNumber);
            if (bay?.Accessories?.CardReader?.IsLocal is true)
            {
                var user = this.usersProvider.Authenticate(bearerToken);
                if (user != null)
                {
                    this.logger.LogInformation($"Login success for user '{user.Name}' by '{this.BayNumber}' local token.");
                    var claims = new UserClaims() { Name = user.Name, AccessLevel = (UserAccessLevel)user.AccessLevel };
                    return this.Ok(claims);
                }
            }

            return this.BadRequest(
                new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Title = "Wms error",
                    Detail = "The Wms is not enabled"
                });
        }

        [HttpPost("authenticate-resource-owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<UserClaims>> AuthenticateWithResourceOwnerPassword(
            string userName,
            string password,
            [FromServices] IWmsSettingsProvider wmsSettingsProvider)
        {
            this.logger.LogDebug(
                "User '{name}': login requested on bay '{number}'.",
                userName,
                this.BayNumber);

            if (wmsSettingsProvider.IsEnabled
                && wmsSettingsProvider.IsConnected
                )
            {
                try
                {
                    this.logger.LogDebug("User '{name}': attempting login through WMS ...", userName);

                    var claims = await this.usersWmsWebService.AuthenticateWithResourceOwnerPasswordAsync(userName, password);

                    if (claims != null)
                    {
                        claims.AccessLevel = (WMS.Data.WebAPI.Contracts.UserAccessLevel)Ferretto.VW.CommonUtils.Messages.Enumerations.UserAccessLevel.Operator;
                    }

                    this.logger.LogInformation(
                        "User '{name}': login successful on bay '{number}' through WMS.",
                        userName,
                        this.BayNumber);

                    return this.Ok(claims);
                }
                catch
                {
                    this.logger.LogWarning(
                        "User '{name}': unable to authenticate on bay '{number}' through WMS.",
                        userName,
                        this.BayNumber);
                }
            }

            var accessLevel = this.usersProvider.Authenticate(userName, password, null);
            if (!accessLevel.HasValue)
            {
                this.logger.LogWarning(
                    "User '{name}': login on '{number}' failed.",
                    userName,
                    this.BayNumber);

                return this.Unauthorized();
            }

            this.logger.LogInformation(
                "User '{name}': login success on bay '{number}'.",
                userName,
                this.BayNumber);

            return this.Ok(
                new UserClaims
                {
                    Name = userName,
                    AccessLevel = (UserAccessLevel)accessLevel.Value,
                });
        }

        [HttpPost("authenticate-support-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<UserClaims>> AuthenticateWithSupportToken(
           string userName,
           string password,
           string supportToken)
        {
            this.logger.LogDebug($"Login requested for user '{userName}' by '{this.BayNumber}'.");

            if (string.IsNullOrEmpty(supportToken))
            {
                return this.Unauthorized();
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

        [HttpPost("change-password")]
        public IActionResult ChangePassword(string userName, string newPassword)
        {
            this.logger.LogInformation($"Change password for user '{userName}' by '{this.BayNumber}'.");

            this.usersProvider.ChangePassword(userName, newPassword);
            return this.Ok();
        }

        [HttpPost("delete-user")]
        public IActionResult DeleteUser(DataModels.UserParameters user)
        {
            this.logger.LogInformation($"Delete user '{user.Name}' by '{this.BayNumber}'.");

            this.usersProvider.DeleteUser(user);
            return this.Ok();
        }

        [HttpGet("get-token-users")]
        public ActionResult<IEnumerable<DataModels.UserParameters>> GetAllTokenUsers()
        {
            var users = this.usersProvider.GetAllTokenUsers();
            return this.Ok(users);
        }

        [HttpPost("all-user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers([FromServices] IWmsSettingsProvider wmsSettingsProvider)
        {
            if (wmsSettingsProvider.IsEnabled
                && wmsSettingsProvider.IsConnected)
            {
                var users = await this.usersWmsWebService.GetAllAsync();

                return this.Ok(users);
            }

            return this.BadRequest(
                new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Title = "Wms error",
                    Detail = wmsSettingsProvider.IsEnabled ? "The Wms is disconnected" : "The Wms is not enabled"
                });
        }

        [HttpGet("get-culture")]
        public ActionResult<IEnumerable<DataModels.UserParameters>> GetAllUserWithCulture()
        {
            var users = this.usersProvider.GetAllUserWithCulture();
            return this.Ok(users);
        }

        [HttpGet("get-disabled")]
        public ActionResult<bool> GetIsDisabled(string userName)
        {
            var users = this.usersProvider.GetIsDisabled(userName);
            return this.Ok(users);
        }

        [HttpGet("get-limited")]
        public ActionResult<bool> GetIsLimited(string userName)
        {
            var users = this.usersProvider.GetIsLimited(userName);
            return this.Ok(users);
        }

        [HttpGet("operator-enabled-wms")]
        public ActionResult<bool> GetOperatorEnabledWithWMS()
        {
            return this.Ok(this.usersProvider.IsOperatorEnabledWithWMS());
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

        [HttpPost("change-disabled")]
        public IActionResult SetIsDisabled(string userName, bool isDisabled)
        {
            this.logger.LogInformation($"Change isDisabled for user '{userName}' to {isDisabled} by '{this.BayNumber}'.");

            this.usersProvider.SetIsDisabled(userName, isDisabled);
            return this.Ok();
        }

        [HttpPost("change-limited")]
        public IActionResult SetIsLimited(string userName, bool isLimited)
        {
            this.logger.LogInformation($"Change isLimited for user '{userName}' to {isLimited} by '{this.BayNumber}'.");

            this.usersProvider.SetIsLimited(userName, isLimited);
            return this.Ok();
        }

        [HttpPost("culture")]
        public void SetMASCulture(CultureInfo culture)
        {
            CommonUtils.Culture.Actual = culture;

            this.logger.LogInformation("Change culture to " + culture.ToString());
        }

        [HttpPost("set-operator-enabled-wms")]
        public IActionResult SetOperatorEnabledWithWMS(bool isEnabled)
        {
            this.usersProvider.SetOperatorEnabledWithWMS(isEnabled);
            return this.Ok();
        }

        [HttpPost("set-culture")]
        public IActionResult SetUserCulture(string culture, string name)
        {
            this.usersProvider.SetUserCulture(culture, name);
            return this.Ok();
        }

        #endregion
    }
}
