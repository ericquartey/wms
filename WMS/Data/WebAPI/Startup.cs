﻿using Ferretto.Common.EF;
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
using NSwag.AspNetCore;

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

            app.UseSwagger();

            if (env.IsProduction())
            {
                app.UseHsts();

                app.UseHttpsRedirection();
            }
            else
            {
                app.UseDeveloperExceptionPage();

                app.UseSwaggerUi3();
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

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "http://localhost:5000";
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

            services.AddSwaggerDocument(settings =>
            {
                settings.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "WMS Data API";
                    document.Info.Description = "REST API for the WMS Data Service";
                };
            });

            services.AddSingleton(a => (a.GetService(
                typeof(IHostingEnvironment)) as IHostingEnvironment)?.ContentRootFileProvider);

            services.AddSingleton<IContentTypeProvider>(new FileExtensionContentTypeProvider());
        }

        #endregion
    }
}
