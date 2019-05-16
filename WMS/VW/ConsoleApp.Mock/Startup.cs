using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
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

            var machineHubName = this.Configuration["Hubs:Machine"];
            if (!string.IsNullOrEmpty(machineHubName))
            {
                app.UseSignalR(routes =>
                {
                    routes.MapHub<MachineHub>($"/{machineHubName}");
                });
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            services.AddSignalR();

            var dataServiceUrl = new System.Uri(this.Configuration["DataService:Url"]);
            var hubPath = this.Configuration["DataService:HubPath"];

            services
                .AddWebApiServices(dataServiceUrl)
                .AddDataHub(new System.Uri(dataServiceUrl, hubPath));

            services.AddHostedService<AutomationService>();
            services.AddSingleton<ILiveMachineDataContext, LiveMachineDataContext>();
            services.AddTransient<IAutomationProvider, AutomationProvider>();
        }

        #endregion
    }
}
