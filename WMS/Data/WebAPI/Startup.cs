﻿using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.WebAPI.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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

            if (env.IsProduction())
            {
                app.UseHsts();

                app.UseHttpsRedirection();
            }
            else
            {
                app.UseSwagger();

                app.UseSwaggerUi3();
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
            if (services == null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            services
                .AddMvc(options =>
                {
                    options.Conventions.Add(new RouteTokenTransformerConvention(
                                                 new SlugifyParameterTransformer()));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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
