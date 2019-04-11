// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace IdentityServer
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

                    // no interactive user, use the clientid/secret for authentication
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
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
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
                    Username = "aorsato",
                    Password = "password",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Alice Orsato"),
                        new Claim(JwtClaimTypes.GivenName, "Alice"),
                        new Claim(JwtClaimTypes.FamilyName, "Orsato"),
                        new Claim(JwtClaimTypes.Email, "aorsato@autoware.it")
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "nmoro",
                    Password = "password",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Nicola Moro"),
                        new Claim(JwtClaimTypes.GivenName, "Nicola"),
                        new Claim(JwtClaimTypes.FamilyName, "Moro"),
                        new Claim(JwtClaimTypes.Email, "nmoro@ferrettogroup.com")
                    }
                },
                new TestUser
                {
                    SubjectId = "3",
                    Username = "gbasso",
                    Password = "password",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Giovanni Basso"),
                        new Claim(JwtClaimTypes.GivenName, "Giovanni"),
                        new Claim(JwtClaimTypes.FamilyName, "Basso"),
                        new Claim(JwtClaimTypes.Email, "gbasso@altran.it")
                    }
                },
                new TestUser
                {
                    SubjectId = "4",
                    Username = "asalomone",
                    Password = "password",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Alessandro Salomone"),
                        new Claim(JwtClaimTypes.GivenName, "Alessandro"),
                        new Claim(JwtClaimTypes.FamilyName, "Salomone"),
                        new Claim(JwtClaimTypes.Email, "asalomone@altran.it")
                    }
                }
            };
        }

        #endregion
    }
}
