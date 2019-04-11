using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.IdentityServer
{
    public class Startup
    {
        #region Constructors

        public Startup(IHostingEnvironment environment)
        {
            this.Environment = environment;
        }

        #endregion

        #region Properties

        public IHostingEnvironment Environment { get; }

        #endregion

        #region Methods

        public void Configure(IApplicationBuilder app)
        {
            if (this.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseMvcWithDefaultRoute();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddTestUsers(Config.GetUsers())
                .AddInMemoryClients(Config.GetClients());

            if (this.Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }
        }

        #endregion
    }
}
