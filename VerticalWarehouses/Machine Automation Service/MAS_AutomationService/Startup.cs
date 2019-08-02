using Ferretto.VW.MAS.AutomationService.Filters;
using Ferretto.VW.MAS.DataLayer.Extensions;
using Ferretto.VW.MAS.InverterDriver;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Ferretto.VW.MAS.IODriver;
using Ferretto.VW.MAS.MissionsManager;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                routes.MapHub<InstallationHub>("/installation-endpoint");
                routes.MapHub<OperatorHub>("/operator-endpoint");
            });

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseDataHub();

            app.UseHealthChecks("/health/ready", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("ready"),
            });

            app.UseHealthChecks("/health/live", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("live"),
            });

            app.UseDataLayer();

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataLayer();

            services
              .AddMvc(options =>
              {
                  options.Filters.Add(typeof(ReadinessFilter));
              })
              .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSignalR();

            services
                .AddHealthChecks()
                .AddCheck<LivelinessHealthCheck>("live", null, tags: new[] { "live" })
                .AddCheck<ReadinessHealthCheck>("ready", null, tags: new[] { "ready" });

            this.InitialiseWmsInterfaces(services);

            services.AddSwaggerDocument(c => c.Title = "Machine Automation Web API");

            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ApiVersionReader = new MediaTypeApiVersionReader(); // read the version number from the accept header
            });

            services.AddSingleton<IEventAggregator, EventAggregator>();

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

            services.AddHostedService<MissionsManagerService>();

            services.AddHostedService<AutomationService>();
        }

        private void InitialiseWmsInterfaces(IServiceCollection services)
        {
            var wmsServiceAddress = this.Configuration.GetDataServiceUrl();
            services.AddWebApiServices(wmsServiceAddress);

            var wmsServiceAddressHubsEndpoint = this.Configuration.GetDataServiceHubUrl();
            services.AddDataHub(wmsServiceAddressHubsEndpoint);
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
