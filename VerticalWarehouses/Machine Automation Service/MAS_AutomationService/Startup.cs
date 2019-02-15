using Ferretto.VW.MAS_AutomationService.Hubs;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Ferretto.VW.MAS_MachineManager;
using Ferretto.VW.MAS_MissionScheduler;
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
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSignalR(routes =>
            {
                routes.MapHub<InstallationHub>($"/installation-endpoint", options => { });
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)

        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR();

            services.AddDbContext<DataLayerContext>(options => options.UseInMemoryDatabase("InMemoryWorkingDB"), ServiceLifetime.Singleton);

            services.AddSingleton<IEventAggregator, EventAggregator>();
            services.AddSingleton<IAutomationService, AutomationService>();

            var connectionString = this.Configuration.GetConnectionString(ConnectionStringName);
            services.AddSingleton<IDataLayer, DataLayer>(provider => new DataLayer(
                connectionString,
                provider.GetService<DataLayerContext>(),
                provider.GetService<IEventAggregator>()));

            services.AddSingleton<IWriteLogService, DataLayer>(provider => provider.GetService<IDataLayer>() as DataLayer);

            services.AddSingleton<IMissionsScheduler, MissionsScheduler>();
            services.AddSingleton<IMachineManager, MachineManager>();
            services.AddSingleton<IFiniteStateMachines, FiniteStateMachines>();
            services.AddSingleton<INewInverterDriver, NewInverterDriver>();
            services.AddSingleton<INewRemoteIODriver, NewRemoteIODriver>();

            //TODO Old InverterDriver Registration to be removed after code refactoring completed
            services.AddSingleton<InverterDriver.IInverterDriver, InverterDriver.InverterDriver>();

            //TODO Old RemoteIODriver Registration to be removed after code refactoring completed
            services.AddSingleton<RemoteIODriver.IRemoteIO, RemoteIODriver.RemoteIO>();
        }

        #endregion
    }
}
