using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Ferretto.IdentityServer
{
    public static class Config
    {
        #region Methods

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("wms-data", "WMS Data Service API"),
                new ApiResource("wms-scheduler", "WMS Scheduler Service API"),
                new ApiResource("vw-automation", "VW Automation Service API"),
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "wms-desktop-client",
                    ClientName = "WMS Desktop App",
                    // use username+password credentials together with the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "wms-data", "wms-scheduler" }
                },
                new Client
                {
                    ClientId = "vw-panel-pc-client",
                    ClientName = "VertiMag PanelPC App",
                    // use username+password credentials together with the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "vw-automation", "wms-data" }
                },
                new Client
                {
                    ClientId = "vw-automation-client",
                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "wms-data", "wms-scheduler" }
                },
                new Client
                {
                    ClientId = "swaggerui",
                    ClientName = "Swagger UI",
                    // use implicit flow (user authentication through Web UI redirection)
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    LogoUri = "https://swagger.io/swagger/media/assets/images/swagger_logo.svg",
                    RedirectUris = { "https://localhost:6001/swagger/oauth2-redirect.html", },
                    PostLogoutRedirectUris = { "http://localhost:6000/swagger/" },

                    AllowedScopes = { "wms-data", "wms-scheduler" }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId()
            };
        }

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "administrator",
                    Password = "password",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Administrator"),
                        new Claim(JwtClaimTypes.GivenName, "Administrator"),
                        new Claim(JwtClaimTypes.FamilyName, "Ferretto"),
                        new Claim(JwtClaimTypes.Email, "administrator@no-domain.com"),
                        new Claim(JwtClaimTypes.Locale, "en-US"),
                        new Claim(JwtClaimTypes.ZoneInfo, "America/New_York")
                    }
                },

                new TestUser
                {
                    SubjectId = "2",
                    Username = "amministratore",
                    Password = "password",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Administrator"),
                        new Claim(JwtClaimTypes.GivenName, "Administrator"),
                        new Claim(JwtClaimTypes.FamilyName, "Ferretto"),
                        new Claim(JwtClaimTypes.Email, "administrator@no-domain.com"),
                        new Claim(JwtClaimTypes.Locale, "it-IT"),
                        new Claim(JwtClaimTypes.ZoneInfo, "Europe/Rome")
                    }
                },
                new TestUser
                {
                    SubjectId = "3",
                    Username = "installer",
                    Password = "password",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Installer"),
                        new Claim(JwtClaimTypes.GivenName, "Installer"),
                        new Claim(JwtClaimTypes.FamilyName, "Ferretto"),
                        new Claim(JwtClaimTypes.Email, "administrator@no-domain.com"),
                        new Claim(JwtClaimTypes.Locale, "en-US")
                    }
                },
                new TestUser
                {
                    SubjectId = "4",
                    Username = "operator",
                    Password = "password",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Operator"),
                        new Claim(JwtClaimTypes.GivenName, "Operator"),
                        new Claim(JwtClaimTypes.FamilyName, "Ferretto"),
                        new Claim(JwtClaimTypes.Email, "administrator@no-domain.com"),
                        new Claim(JwtClaimTypes.Locale, "en-US")
                    }
                },
            };
        }

        #endregion
    }
}
