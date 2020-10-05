using System.Collections.Generic;
using System.Threading.Tasks;
using DevExpress.XtraRichEdit.Model;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Microsoft.AppCenter.Analytics;

namespace Ferretto.VW.App.Services
{
    internal sealed class AuthenticationService : IAuthenticationService
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineUsersWebService usersService;

        #endregion

        #region Constructors

        public AuthenticationService(
            IMachineUsersWebService usersService,
            IBayManager bayManager)
        {
            this.usersService = usersService;
            this.bayManager = bayManager;
        }

        #endregion

        #region Events

        public event System.EventHandler<UserAuthenticatedEventArgs> UserAuthenticated;

        #endregion

        #region Properties

        public UserAccessLevel AccessLevel { get; private set; }

        public bool IsAutoLogoutServiceUser { get; set; }

        public string UserName { get; private set; }

        #endregion

        #region Methods

        public async Task<string> GetToken()
        {
            return await this.usersService.GetSupportTokenAsync();
        }

        public async Task<UserClaims> LogInAsync(string userName, string password, string supportToken)
        {
            var userClaims = string.IsNullOrEmpty(supportToken)
                ? await this.usersService.AuthenticateWithResourceOwnerPasswordAsync(userName, password)
                : await this.usersService.AuthenticateWithSupportTokenAsync(userName, password, supportToken);

            this.UserName = userClaims.Name;
            this.AccessLevel = userClaims.AccessLevel;
            this.UserAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs(userName, this.AccessLevel));

            Analytics.TrackEvent("Login", new Dictionary<string, string>
            {
                 { "Authentication Mode", supportToken != null ? "Support Token" : "Password" },
                 { "Machine Serial Number", this.bayManager.Identity?.SerialNumber }
            });

            return userClaims;
        }

        public async Task<UserClaims> LogInAsync(string bearerToken)
        {
            var userClaims = await this.usersService
              .AuthenticateWithBearerTokenAsync(bearerToken);

            this.UserName = userClaims.Name;
            switch (userClaims.AccessLevel)
            {
                case UserAccessLevel.NoAccess:
                    //this.AccessLevel = UserAccessLevel.Support;
                    this.AccessLevel = UserAccessLevel.Operator;
                    break;

                case UserAccessLevel.Operator:
                    //this.AccessLevel = UserAccessLevel.Admin;
                    this.AccessLevel = UserAccessLevel.Operator;
                    break;

                case UserAccessLevel.Installer:
                    //this.AccessLevel = UserAccessLevel.Installer;
                    this.AccessLevel = UserAccessLevel.Operator;
                    break;

                case UserAccessLevel.Support:
                    this.AccessLevel = UserAccessLevel.Operator;
                    break;

                default:
                    this.AccessLevel = UserAccessLevel.NoAccess;
                    break;
            }
            this.UserAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs(userClaims.Name, this.AccessLevel));

            Analytics.TrackEvent("Login", new Dictionary<string, string>
            {
                 { "Authentication Mode", "Bearer Token" },
                 { "Machine Serial Number", this.bayManager.Identity?.SerialNumber }
            });

            return userClaims;
        }

        public async Task LogOutAsync()
        {
            Analytics.TrackEvent("Logout", new Dictionary<string, string>
            {
                 { "Machine Serial Number", this.bayManager.Identity?.SerialNumber }
            });

            await Task.Run(() => this.UserName = null);
        }

        #endregion
    }
}
