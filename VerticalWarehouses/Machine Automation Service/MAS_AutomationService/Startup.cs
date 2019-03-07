using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_AutomationService.Hubs;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Ferretto.VW.MAS_IODriver;
using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_MissionsManager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prism.Events;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Ferretto.VW.MAS_AutomationService
{
    public class Startup
    {
        #region Fields

        private const string ConnectionStringName = "AutomationService";

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
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseSignalR(routes => { routes.MapHub<InstallationHub>("/installation-endpoint", options => { }); });

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)

        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR();

            var connectionString = this.Configuration.GetConnectionString(ConnectionStringName);
            services.AddDbContext<DataLayerContext>(options => options.UseInMemoryDatabase("InMemoryWorkingDB"),
                ServiceLifetime.Singleton);

            services.AddSingleton<IEventAggregator, EventAggregator>();

            services.AddSingleton<IDataLayer, DataLayer>(provider => new DataLayer(
                connectionString,
                provider.GetService<DataLayerContext>(),
                provider.GetService<IEventAggregator>()));

            services.AddSingleton<IWriteLogService, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IHostedService, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            // Begin changes - 06/03/2019
            services.AddSingleton<IDataLayerCellManagment, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IDataLayerValueManagment, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);
            // End changes - 06/03/2019

            services.AddSingleton<ISocketTransport, SocketTransport>();

            this.RegisterRemoteIODriver(services);

            this.RegisterInverterDriver(services);

            ////TODO Old InverterDriver Registration to be removed after code refactoring completed
            //services.AddSingleton<IInverterDriver, InverterDriver.InverterDriver>();

            ////TODO Old RemoteIODriver Registration to be removed after code refactoring completed
            //services.AddSingleton<IRemoteIO, RemoteIO>();
            services.AddSingleton<IModbusTransport, ModbusTransport>();

            services.AddHostedService<HostedIoDriver>();

            services.AddHostedService<HostedInverterDriver>();

            services.AddHostedService<FiniteStateMachines>();

            services.AddHostedService<MissionsManager>();

            services.AddHostedService<AutomationService>();
        }

        private void RegisterInverterDriver(IServiceCollection services)
        {
            var useMockedInverterDriver = this.Configuration.GetValue<bool>("Vertimag:InverterDriver:UseMock");
            if (useMockedInverterDriver)
            {
                services.AddHostedService<HostedInverterDriverMock>();
                services.AddSingleton<INewInverterDriver, NewInverterDriverMock>();
            }
            else
            {
                services.AddSingleton<INewInverterDriver, NewInverterDriver>();
                services.AddHostedService<HostedInverterDriver>();
            }
        }

        private void RegisterRemoteIODriver(IServiceCollection services)
        {
            var useRemoteIODriver = this.Configuration.GetValue<bool>("Vertimag:RemoteIODriver:UseMock");
            if (useRemoteIODriver)
                services.AddSingleton<INewRemoteIODriver, NewRemoteIODriverMock>();
            else
                services.AddSingleton<INewRemoteIODriver, NewRemoteIODriver>();
        }

        #endregion
    }
}
