using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Telemetry.Contracts.Hub;

namespace Ferretto.VW.App.Services
{
    internal sealed class AuthenticationService : IAuthenticationService
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly ITelemetryHubClient telemetryHubClient;

        private readonly IMachineUsersWebService usersService;

        #endregion

        #region Constructors

        public AuthenticationService(
            IMachineUsersWebService usersService,
            IBayManager bayManager,
            ITelemetryHubClient telemetryHubClient)
        {
            this.usersService = usersService;
            this.bayManager = bayManager;
            this.telemetryHubClient = telemetryHubClient ?? throw new ArgumentNullException(nameof(telemetryHubClient));
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

        public async Task<UserClaims> LogInAsync(string userName, string password, string supportToken, UserAccessLevel accessLevel)
        {
            var userClaims = string.IsNullOrEmpty(supportToken)
                ? await this.usersService.AuthenticateWithResourceOwnerPasswordAsync(userName, password)
                : await this.usersService.AuthenticateWithSupportTokenAsync(userName, password, supportToken);

            this.UserName = userClaims.Name;
            if (accessLevel != UserAccessLevel.NoAccess)
            {
                userClaims.AccessLevel = accessLevel;
            }

            this.AccessLevel = userClaims.AccessLevel;

            if (this.AccessLevel == UserAccessLevel.NoAccess)
            {
                throw new Exception(Resources.Localized.Get("LoadLogin.NoAccessLevel"));
            }

            this.UserAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs(userName, this.AccessLevel));

            await this.TelemetryLoginLogoutAsync("Login", supportToken);

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

                case UserAccessLevel.Movement:
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

            userClaims.AccessLevel = this.AccessLevel;

            if (this.AccessLevel == UserAccessLevel.NoAccess)
            {
                throw new Exception(Resources.Localized.Get("LoadLogin.NoAccessLevel"));
            }

            this.UserAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs(userClaims.Name, this.AccessLevel));

            await this.TelemetryLoginLogoutAsync("Login", bearerToken);

            return userClaims;
        }

        public async Task LogOutAsync()
        {
            await this.TelemetryLoginLogoutAsync("Logout");
            await Task.Run(() => this.UserName = null);
        }

        private async Task TelemetryLoginLogoutAsync(string action, string supportToken = "")
        {
            var bay = await this.bayManager.GetBayAsync();
            var ip = "";

            try
            {
                var allnetworkAdapter = NetworkInterface.GetAllNetworkInterfaces();
                var networkInterface = allnetworkAdapter.Where(s => s.NetworkInterfaceType == NetworkInterfaceType.Ethernet).FirstOrDefault();
                var properties = networkInterface.GetIPProperties();
                var ipProp = properties.UnicastAddresses.FirstOrDefault(i => i.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                ip = ipProp.Address.ToString();
            }
            catch (Exception) { }

            var errorLog = new ErrorLog
            {
                AdditionalText = $"{action} {this.AccessLevel} {supportToken} {this.AccessLevel} (Ip: {ip})",
                BayNumber = (int)bay.Number,
                Code = 0,
                DetailCode = (int)this.AccessLevel,
                ErrorId = int.Parse(DateTime.Now.ToString("-MMddHHmmss")),
                InverterIndex = 0,
                OccurrenceDate = DateTimeOffset.Now,
                ResolutionDate = null
            };

            await this.telemetryHubClient.SendErrorLogAsync(errorLog);
        }

        #endregion
    }
}
