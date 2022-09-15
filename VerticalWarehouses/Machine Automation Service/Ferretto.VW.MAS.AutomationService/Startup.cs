using System;
using System.Globalization;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.MAS.AutomationService.Filters;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DeviceManager;
using Ferretto.VW.MAS.InverterDriver;
using Ferretto.VW.MAS.IODriver;
using Ferretto.VW.MAS.MachineManager;
using Ferretto.VW.MAS.MissionManager;
using Ferretto.VW.MAS.SocketLink;
using Ferretto.VW.MAS.TimeManagement;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.Telemetry.Contracts.Hub;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Prism.Events;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Ferretto.VW.CommonUtils.Converters;
using Microsoft.Extensions.Hosting;

namespace Ferretto.VW.MAS.AutomationService
{
    public class Startup
    {
        #region Constructors

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            this.Configuration = configuration;
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        public ILogger<Startup> Logger { get; }

        #endregion

        #region Methods

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            appLifetime?.ApplicationStopped.Register(this.OnStopped);

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

            app.UseMessageLogging();

            if (!env.IsProduction())
            {
                SwaggerBuilderExtensions.UseSwagger(app);
                app.UseSwaggerUI(config =>
                {
                    config.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });
            }

            app.UseMasHealthChecks();

            app.UseDataLayer();

            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<InstallationHub>("/installation-endpoint");
                endpoints.MapHub<OperatorHub>("/operator-endpoint");
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataLayer();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(ReadinessFilter));
                options.Filters.Add(typeof(BayNumberActionFilter));
                options.Filters.Add(typeof(ExceptionsFilter));
                options.Conventions.Add(
                    new RouteTokenTransformerConvention(
                      new SlugifyParameterTransformer()));
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new IPAddressConverter());
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddMasHealthChecks();

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "Machine Automation Web API",
                        Version = "v1",
                    });
                config.OperationFilter<BayNumberOperationFilter>();
                config.CustomSchemaIds(i => i.FullName);
            });

            services.AddSingleton<IEventAggregator, EventAggregator>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .SetIsOriginAllowed(x => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            AddWmsServices(services);

            services
                .AddIODriver()
                .AddInverterDriver()
                .AddFiniteStateMachines()
                .AddMachineManager()
                .AddMissionManager();

            //services.AddInternalTimingServices();

            services.AddHostedService<NotificationTelemetryService>();
            services.AddHostedService<NotificationRelayService>();

            services.AddScoped<IInverterProvider, InverterProvider>();
            services.AddScoped<IIoDeviceProvider, IoDeviceProvider>();
            services.AddScoped<IConfigurationProvider, ConfigurationProvider>();

            var telemetryUrl = this.Configuration.GetValue<string>("Telemetry:Url");
            services.AddTelemetryHub(new Uri(telemetryUrl));
            services.AddSignalR()
                .AddNewtonsoftJsonProtocol();
        }

        private static void AddWmsServices(IServiceCollection services)
        {
            services.AddWmsWebServices(
                s => s.GetRequiredService<IWmsSettingsProvider>().ServiceUrl,
                s =>
                {
                    var dataLayerService = s.GetRequiredService<IDataLayerService>();

                    return dataLayerService.IsReady
                        ? s.GetRequiredService<IMachineProvider>().GetIdentity()
                        : 0;
                });

            services.AddWmsDataHub();

            services.AddWmsMissionManager();

            services.AddTimeServices();

            services.AddSocketLinkServices();
        }

        private void OnStopped()
        {
            this.Logger.LogInformation("Application stopped");
        }

        #endregion
    }
}
