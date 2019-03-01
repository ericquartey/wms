using Ferretto.VW.Common_Utils.Interfaces;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_AutomationService.Hubs;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_MissionsManager;
using Ferretto.VW.RemoteIODriver;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

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

            services.AddHostedService<AutomationService>();
            services.AddHostedService<MissionsManager>();
            services.AddHostedService<FiniteStateMachines>();

            this.RegisterInverterDriver(services);

            this.RegisterRemoteIODriver(services);

            services.AddSingleton<IEventAggregator, EventAggregator>();
            services.AddSingleton<IDataLayer, DataLayer>(provider => new DataLayer(
                connectionString,
                provider.GetService<DataLayerContext>(),
                provider.GetService<IEventAggregator>()));

            services.AddSingleton<IWriteLogService, DataLayer>(provider =>
                provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<ISocketTransport, SocketTransport>();
            services.AddSingleton<IModbusTransport, ModbusTransport>();

            //TODO Old InverterDriver Registration to be removed after code refactoring completed
            services.AddSingleton<IInverterDriver, InverterDriver.InverterDriver>();

            //TODO Old RemoteIODriver Registration to be removed after code refactoring completed
            services.AddSingleton<IRemoteIO, RemoteIO>();
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
