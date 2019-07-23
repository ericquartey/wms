using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.InverterDriver;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Ferretto.VW.MAS.IODriver;
using Ferretto.VW.MAS.Utils.Utilities;
using Ferretto.VW.MAS_Utils.Utilities;
using Ferretto.VW.MAS_Utils.Utilities.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService
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
                routes.MapHub<InstallationHub>("/installation-endpoint", options => { });
                routes.MapHub<OperatorHub>("/operator-endpoint", options => { });
            });

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSignalR();

            var dataLayerConfiguration = new DataLayerConfiguration(
                this.Configuration.GetDataLayerPrimaryConnectionString(),
                this.Configuration.GetDataLayerSecondaryConnectionString(),
                this.Configuration.GetDataLayerConfigurationFile());

            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ApiVersionReader = new MediaTypeApiVersionReader(); // read the version number from the accept header
            });

            var wmsServiceAddress = new System.Uri(this.Configuration.GetDataServiceUrl());
            var wmsServiceAddressHubsEndpoint = new System.Uri(this.Configuration.GetDataServiceHubUrl());

            services.AddSingleton<IEventAggregator, EventAggregator>();

            services.AddSingleton<IBaysManager, BaysManager>();

            this.RegisterDataLayer(services, dataLayerConfiguration);

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

            services.AddHostedService<FiniteStateMachines.FiniteStateMachines>();

            // HACK commented out module initialization for development purpose
            //services.AddHostedService<MissionsManager>();
            services.AddHostedService<AutomationService>();

            services.AddWebApiServices(wmsServiceAddress);

            services.AddDataHub(wmsServiceAddressHubsEndpoint);
        }

        private void RegisterDataLayer(IServiceCollection services, DataLayerConfiguration dataLayerConfiguration)
        {
            services.AddSingleton<IDataLayer, DataLayer.DataLayer>(provider => new DataLayer.DataLayer(
                            dataLayerConfiguration,
                            provider.GetService<IEventAggregator>(),
                            provider.GetService<ILogger<DataLayer.DataLayer>>()));

            services.AddSingleton<IBayPositionControl, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IBeltBurnishing, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<ICellControl, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IGeneralInfo, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IHorizontalAxis, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IHorizontalManualMovements, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IHorizontalMovementBackwardProfile, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IHorizontalMovementForwardProfile, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<ILoadFirstDrawer, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IOffsetCalibration, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IPanelControl, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IResolutionCalibration, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<ISetupNetwork, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<ISetupStatus, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IShutterHeightControl, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IVerticalAxis, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IVerticalManualMovements, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IWeightControl, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IHostedService, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IDataLayerCellManagement, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IDataLayerConfigurationValueManagment, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);

            services.AddSingleton<IVertimagConfiguration, DataLayer.DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer.DataLayer);
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
