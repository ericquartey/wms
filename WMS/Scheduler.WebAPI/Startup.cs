using Ferretto.Common.BusinessProviders;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;
using Ferretto.WMS.Scheduler.WebAPI.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
using NSwag.AspNetCore;

namespace Ferretto.WMS.Scheduler.WebAPI
{
    public class Startup
    {
        #region Constructors

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        #endregion Constructors

        #region Properties

        public IConfiguration Configuration { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        ///  This method gets called by the runtime.
        ///  Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwaggerUi3WithApiExplorer(settings =>
                {
                    settings.PostProcess = document =>
                    {
                        document.Info.Version = "v1";
                        document.Info.Title = "WMS Scheduler API";
                        document.Info.Description = "REST API for the WMS Scheduler";
                    };
                    settings.GeneratorSettings.DefaultPropertyNameHandling =
                        PropertyNameHandling.CamelCase;
                });
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

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

            Core.ServiceLocator.Instance = app.ApplicationServices;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var connectionString = this.Configuration.GetConnectionString("WmsConnectionString");
            services.AddDbContext<DatabaseContext>(
                options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Ferretto.Common.EF")));

            services.AddBusinessProviders();
            services.AddCoreBusinessProviders();

            var schedulerUrl = this.Configuration["Scheduler:Url"];
            services.AddWebAPIClients(schedulerUrl);

            services.AddTransient<IWarehouse, Warehouse>();

            services.AddSignalR();
        }

        #endregion Methods
    }
}
