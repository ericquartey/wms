using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    internal class AuthenticationService : IAuthenticationService
    {
        #region Fields

        private readonly HttpClient client;

        private readonly Uri serviceUrl;

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

        public async Task LoginAsync(string userName, string password)
        {
            using (var identityClient = new HttpClient())
            {
                var discoveryDocument = await identityClient.GetDiscoveryDocumentAsync(this.serviceUrl.AbsoluteUri);
                if (discoveryDocument.IsError)
                {
                    throw new HttpRequestException(discoveryDocument.Error);
                }

                this.TokenResponse = await identityClient.RequestPasswordTokenAsync(
                     new PasswordTokenRequest
                     {
                         Address = discoveryDocument.TokenEndpoint,
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
