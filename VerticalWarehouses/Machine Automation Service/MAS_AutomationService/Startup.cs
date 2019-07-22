using Ferretto.VW.MAS.AutomationService;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS_AutomationService.Hubs;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Ferretto.VW.MAS_IODriver;
using Ferretto.VW.MAS_Utils.Utilities;
using Ferretto.VW.MAS_Utils.Utilities.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_AutomationService
{
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSignalR(routes =>
            {
                routes.MapHub<InstallationHub>("/installation-endpoint");
                routes.MapHub<OperatorHub>("/operator-endpoint");
            });

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSignalR();

            services.AddSwaggerDocument(c => c.Title = "Machine Automation Web API");

            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ApiVersionReader = new MediaTypeApiVersionReader(); // read the version number from the accept header
            });

            services.AddSingleton<IEventAggregator, EventAggregator>();

            services.AddSingleton<IBaysManager, BaysManager>();

            this.RegisterDataLayer(services);

            this.RegisterSocketTransport(services);

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.AddHostedService<HostedSHDIoDriver>();

            services.AddHostedService<HostedInverterDriver>();

            services.AddHostedService<FiniteStateMachines>();

            // HACK commented out module initialization for development purpose
            // services.AddHostedService<MissionsManager>();

            services.AddHostedService<AutomationService>();

            var wmsServiceAddress = new System.Uri(this.Configuration.GetDataServiceUrl());
            services.AddWebApiServices(wmsServiceAddress);

            var wmsServiceAddressHubsEndpoint = new System.Uri(this.Configuration.GetDataServiceHubUrl());
            services.AddDataHub(wmsServiceAddressHubsEndpoint);
        }

        private void RegisterDataLayer(IServiceCollection services)
        {
            services.AddDbContext<DataLayerContext>(
                options => options.UseSqlite(this.Configuration.GetDataLayerPrimaryConnectionString()),
                ServiceLifetime.Singleton);

            var dataLayerConfiguration = new DataLayerConfiguration(
                this.Configuration.GetDataLayerSecondaryConnectionString(),
                this.Configuration.GetDataLayerConfigurationFile());

            services.AddSingleton<IDataLayer, DataLayer>(provider => new DataLayer(
                dataLayerConfiguration,
                provider.GetService<DataLayerContext>(),
                provider.GetService<IEventAggregator>(),
                provider.GetService<ILogger<DataLayer>>()));

            services.AddSingleton<IBayPositionControlDataLayer, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IBeltBurnishingDataLayer, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<ICellControlDataLayer, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IGeneralInfoDataLayer, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IHorizontalAxis, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IHorizontalManualMovements, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IHorizontalMovementBackwardProfile, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IHorizontalMovementForwardProfile, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<ILoadFirstDrawer, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IOffsetCalibration, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IPanelControl, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IResolutionCalibration, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<ISetupNetwork, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<ISetupStatus, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IShutterHeightControl, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IVerticalAxis, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IVerticalManualMovements, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IWeightControl, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IHostedService, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<ICellManagmentDataLayer, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IConfigurationValueManagmentDataLayer, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IResolutionConversion, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IRuntimeValueManagmentDataLayer, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IVertimagConfiguration, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IErrorStatisticsDataLayer, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IMachineStatisticsDataLayer, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<ILoadingUnitStatistics, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);
        }

        private void RegisterSocketTransport(IServiceCollection services)
        {
            var useMockedTransport = this.Configuration.UseInverterDriverMock();
            if (useMockedTransport)
            {
                services.AddSingleton<ISocketTransport, SocketTransportMock>();
            }
            else
            {
                services.AddSingleton<ISocketTransport, SocketTransport>();
            }
        }

        #endregion
    }
}
