// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
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

                    RedirectUris = { "http://localhost:5000/swagger/o2c.html" },
                    PostLogoutRedirectUris = { "http://localhost:5000/swagger/" },

                    AllowedScopes = { "wms-data" }
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
                    Password = "password"
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "nmoro",
                    Password = "password"
                },
                new TestUser
                {
                    SubjectId = "3",
                    Username = "gbasso",
                    Password = "password"
                },
                new TestUser
                {
                    SubjectId = "4",
                    Username = "asalomone",
                    Password = "password"
                }
            };
        }

        #endregion
    }
}
