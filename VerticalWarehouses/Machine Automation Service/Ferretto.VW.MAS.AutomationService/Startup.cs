using System.Globalization;
using Ferretto.VW.MAS.AutomationService.Filters;
using Ferretto.VW.MAS.AutomationService.Interfaces;
using Ferretto.VW.MAS.AutomationService.Provider;
using Ferretto.VW.MAS.DataLayer.Extensions;
using Ferretto.VW.MAS.FiniteStateMachines;
using Ferretto.VW.MAS.InverterDriver;
using Ferretto.VW.MAS.InverterDriver.Extensions;
using Ferretto.VW.MAS.IODriver;
using Ferretto.VW.MAS.MissionsManager;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService
{
    public class Startup
    {
        #region Fields

        private const string LiveHealthCheckTag = "live";

        private const string ReadyHealthCheckTag = "ready";

        #endregion

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("it"),
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
            });

            app.UseSignalR(routes =>
            {
                routes.MapHub<InstallationHub>("/installation-endpoint");
                routes.MapHub<OperatorHub>("/operator-endpoint");
            });

            app.UseOpenApi();
            app.UseSwaggerUi3();

            if (this.Configuration.IsWmsEnabled())
            {
                app.UseDataHub();
            }

            app.UseHealthChecks("/health/ready", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains(ReadyHealthCheckTag),
            });

            app.UseHealthChecks("/health/live", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains(LiveHealthCheckTag),
            });

            app.UseDataLayer();

            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataLayer();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services
              .AddMvc(options =>
              {
                  options.Filters.Add(typeof(ReadinessFilter));
                  options.Filters.Add(typeof(BayNumberFilter));
                  options.Filters.Add(typeof(ExceptionsFilter));
                  options.Conventions.Add(
                      new RouteTokenTransformerConvention(
                        new SlugifyParameterTransformer()));
              })
              .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSignalR();

            services
                .AddHealthChecks()
                .AddCheck<LivelinessHealthCheck>("liveliness-check", null, tags: new[] { LiveHealthCheckTag })
                .AddCheck<ReadinessHealthCheck>("readiness-check", null, tags: new[] { ReadyHealthCheckTag });

            services.AddSwaggerDocument(c => c.Title = "Machine Automation Web API");

            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ApiVersionReader = new MediaTypeApiVersionReader(); // read the version number from the accept header
            });

            services.AddSingleton<IEventAggregator, EventAggregator>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            this.InitialiseWmsInterfaces(services);

            services
                .AddIODriver()
                .AddInverterDriver()
                .AddFiniteStateMachines()
                .AddMissionsManager();

            services.AddHostedService<AutomationService>();

            services.AddTransient<IInverterProvider, InverterProvider>();
            services.AddTransient<IIoDeviceProvider, IoDeviceProvider>();
        }

        private void InitialiseWmsInterfaces(IServiceCollection services)
        {
            var wmsServiceAddress = this.Configuration.GetWmsServiceUrl();
            services.AddWebApiServices(wmsServiceAddress);

            var wmsServiceAddressHubsEndpoint = this.Configuration.GetWmsServiceHubUrl();
            services.AddDataHub(wmsServiceAddressHubsEndpoint);
        }

        #endregion
    }
}
