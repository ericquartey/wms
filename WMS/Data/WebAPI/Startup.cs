using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Providers;
using Ferretto.WMS.Data.WebAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#if DEBUG
using NSwag.AspNetCore;
#endif

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

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
#if DEBUG
                app.UseRequestResponseLogging();

                app.UseSwaggerUi3WithApiExplorer(settings =>
                {
                    settings.PostProcess = document =>
                    {
                        var assembly = typeof(Startup).Assembly;
                        var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

                        document.Info.Version = versionInfo.FileVersion;
                        document.Info.Title = "WMS Data API";
                        document.Info.Description = "REST API for the WMS Data Service";
                    };
                    settings.GeneratorSettings.DefaultPropertyNameHandling =
                        NJsonSchema.PropertyNameHandling.CamelCase;

                    settings.GeneratorSettings.DefaultEnumHandling = NJsonSchema.EnumHandling.String;
                });
#endif
            }
            else if (env.IsProduction())
            {
                app.UseHsts();

                app.UseHttpsRedirection();
            }

            var wakeupHubEndpoint = this.Configuration["Hubs:WakeUp"];
            if (string.IsNullOrWhiteSpace(wakeupHubEndpoint) == false)
            {
                app.UseSignalR(routes =>
                {
                    routes.MapHub<WakeupHub>($"/{wakeupHubEndpoint}");
                });
            }

            var healthHubEndpoint = this.Configuration["Hubs:Health"];
            if (string.IsNullOrWhiteSpace(healthHubEndpoint) == false)
            {
                app.UseSignalR(routes =>
                {
                    routes.MapHub<HealthHub>($"/{healthHubEndpoint}");
                });
            }

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var connectionString = this.Configuration.GetConnectionString("WmsConnectionString");
            services.AddDbContext<DatabaseContext>(
                options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Ferretto.Common.EF")));

            services.AddMemoryCache();

            services.AddDataServiceProviders();

            services.AddSchedulerServiceProviders();

            services.AddSignalR();
        }

        #endregion
    }
}
