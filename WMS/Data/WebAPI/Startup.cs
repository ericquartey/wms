using System.Collections.Generic;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.WebAPI.Hubs;
using Ferretto.WMS.Scheduler.Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace Ferretto.WMS.Data.WebAPI
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This class register services into container")]
    public class Startup
    {
        #region Constructors

        public Startup(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            this.Configuration = configuration;
        }

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        #endregion

        #region Methods

        /// <summary>
        ///  This method gets called by the runtime.
        ///  Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (app == null)
            {
                throw new System.ArgumentNullException(nameof(app));
            }

            if (env == null)
            {
                throw new System.ArgumentNullException(nameof(env));
            }

            if (env.IsProduction())
            {
                app.UseHsts();

                app.UseHttpsRedirection();
            }
            else
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(
#pragma warning disable S1075 // URIs should not be hardcoded
                        "/swagger/v1/swagger.json",
#pragma warning restore S1075 // URIs should not be hardcoded
                        "WMS API v1");

                    options.OAuthClientId("swaggerui");
                    options.OAuth2RedirectUrl(
#pragma warning disable S1075 // URIs should not be hardcoded
                        "http://localhost:5000/swagger/o2c.html");
#pragma warning restore S1075 // URIs should not be hardcoded
                });
            }

            app.UseAuthentication();

            var wakeupHubEndpoint = this.Configuration["Hubs:WakeUp"];
            if (string.IsNullOrWhiteSpace(wakeupHubEndpoint) == false)
            {
                app.UseSignalR(routes =>
                {
                    routes.MapHub<WakeupHub>($"/{wakeupHubEndpoint}");
                });
            }

            var schedulerHubEndpoint = this.Configuration["Hubs:Scheduler"];
            if (string.IsNullOrWhiteSpace(schedulerHubEndpoint) == false)
            {
                app.UseSignalR(routes =>
                {
                    routes.MapHub<SchedulerHub>($"/{schedulerHubEndpoint}");
                });
            }

            app.UseHealthChecks($"/health");

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var identityServerUrl = this.Configuration.GetValue<string>("IdentityServer:Url");

            services
                .AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = identityServerUrl;
                    options.RequireHttpsMetadata = false;
                    options.Audience = "wms-data";
                });

            services
                .AddDataServiceProviders()
                .AddSchedulerServiceProviders();

            var connectionString = this.Configuration.GetConnectionString("WmsConnectionString");
            services
                .AddMemoryCache()
                .AddDbContextPool<DatabaseContext>(
                    options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Ferretto.Common.EF")));

            services.AddHealthChecks();
            services.AddSignalR();

            services.ConfigureSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName);
            });
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "WMS API",
                    Version = "v1"
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();

                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = $"{identityServerUrl}/connect/authorize",
                    TokenUrl = $"{identityServerUrl}/connect/token",
                    Scopes = new Dictionary<string, string>
                    {
                        { "wms-data", "WMS Data API" }
                    }
                });
            });

            services.AddSingleton(a => (a.GetService(
                typeof(IHostingEnvironment)) as IHostingEnvironment)?.ContentRootFileProvider);

            services.AddSingleton<IContentTypeProvider>(new FileExtensionContentTypeProvider());
        }

        #endregion
    }
}
