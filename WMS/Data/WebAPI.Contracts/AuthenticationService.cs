using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    internal class AuthenticationService : IAuthenticationService
    {
        #region Fields

        private readonly HttpClient client;

        private readonly Uri serviceUrl;

        private DiscoveryResponse discoveryResponse;

        #endregion

        #region Constructors

        public AuthenticationService(Uri serviceUrl, HttpClient client)
        {
            this.serviceUrl = serviceUrl;
            this.client = client;
        }

        #endregion

        #region Properties

        public string AccessToken => this.TokenResponse?.AccessToken;

        public TokenResponse TokenResponse { get; private set; }

        #endregion

        #region Methods

        public async Task<string> GetUserNameAsync()
        {
            if (this.discoveryResponse == null)
            {
                return null;
            }

            using (var identityClient = new HttpClient())
            {
                var request = new UserInfoRequest
                {
                    Address = this.discoveryResponse.UserInfoEndpoint,
                    Token = this.AccessToken
                };

                identityClient.SetBearerToken(this.AccessToken);
                var userInfo = await identityClient.GetUserInfoAsync(request);
                if (userInfo.IsError)
                {
                    throw new HttpRequestException(userInfo.Error);
                }

                return userInfo.Claims.SingleOrDefault(c => c.Type == JwtClaimTypes.Name)?.Value;
            }
        }

        public async Task LoginAsync(string userName, string password)
        {
            using (var identityClient = new HttpClient())
            {
                this.discoveryResponse = await identityClient.GetDiscoveryDocumentAsync(this.serviceUrl.AbsoluteUri);
                if (this.discoveryResponse.IsError)
                {
                    throw new HttpRequestException(this.discoveryResponse.Error);
                }

                this.TokenResponse = await identityClient.RequestPasswordTokenAsync(
                     new PasswordTokenRequest
                     {
                         Address = this.discoveryResponse.TokenEndpoint,
                         ClientId = "vw-panel-pc-client",
                         ClientSecret = "secret",

                         UserName = userName,
                         Password = password,
                         Scope = "wms-data"
                     });

                if (this.TokenResponse.IsError)
                {
                    throw new HttpRequestException($"Unable to log in {this.TokenResponse.Error}.");
                }
            }

            this.client.SetBearerToken(this.TokenResponse.AccessToken);
        }

        #endregion
    }
}
