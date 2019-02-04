using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MAS_DataLayer;
using Microsoft.EntityFrameworkCore;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_MachineManager;
using Ferretto.VW.MAS_MissionScheduler;

namespace Ferretto.VW.MAS_AutomationService
{
    public class Startup
    {
        #region Fields

        private const string ConnectionStringName = "AutomationService";

        #endregion Fields

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

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var connectionString = this.Configuration.GetConnectionString(ConnectionStringName);

            services.AddDbContext<DataLayerContext>(options => options.UseSqlite(connectionString));

            services.AddSingleton<IAutomationService, AutomationService>();
            services.AddSingleton<IWriteLogService, WriteLogService>();
            services.AddSingleton<IMissionsScheduler, MissionsScheduler>();
            services.AddSingleton<IMachineManager, MachineManager>();
            services.AddSingleton<IFiniteStateMachines, FiniteStateMachines>();
            services.AddSingleton<IInverterDriver, MAS_InverterDriver.InverterDriver>();
        }

        #endregion Methods
    }
}
