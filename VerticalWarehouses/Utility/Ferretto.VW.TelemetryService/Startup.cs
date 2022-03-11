using System;
using Ferretto.VW.TelemetryService.Data;
using Ferretto.VW.TelemetryService.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ferretto.VW.TelemetryService
{
    public class Startup
    {
        #region Fields

        private const string ConnectionStringName = "Database";

        private const string SchemaVersionName = "SchemaVersion";

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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<TelemetryHub>("/telemetry");
            });

            app.ApplicationServices
                .GetRequiredService<ITelemetryWebHubClient>()
                .ConnectAsync();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = null;
            });

            services.AddDatabase();

            // SENZA PROXY
            //services.AddSingleton<ITelemetryWebHubClient>(
            //    s => new TelemetryWebHubClient(
            //        this.Configuration.GetValue<Uri>("Telemetry:Url"),
            //        s.GetRequiredService<IServiceScopeFactory>()));

            // CON PROXY
            var proxy = new System.Net.WebProxy(this.Configuration.GetValue<Uri>("Telemetry:Proxy:Url"));
            proxy.Credentials = new System.Net.NetworkCredential(this.Configuration.GetValue<string>("Telemetry:Proxy:User"), this.Configuration.GetValue<string>("Telemetry:Proxy:Password"));
            services.AddSingleton<ITelemetryWebHubClient>(
                s => new TelemetryWebHubClient(
                    this.Configuration.GetValue<Uri>("Telemetry:Url"),
                    s.GetRequiredService<IServiceScopeFactory>(), proxy));

            services.AddMemoryCache();

            services.AddScoped<IMachineProvider, MachineProvider>();
            services.AddScoped<IErrorLogProvider, ErrorLogProvider>();
            services.AddScoped<IIOLogProvider, IOLogProvider>();
            services.AddScoped<IMissionLogProvider, MissionLogProvider>();
            services.AddScoped<IServicingInfoProvider, ServicingInfoProvider>();
            services.AddScoped<IScreenShotProvider, ScreenShotProvider>();

            services.AddHostedService<DatabaseCleanupService>();
        }

        #endregion
    }
}
