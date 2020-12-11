using System;
using Ferretto.VW.TelemetryService.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Realms;

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

            services.AddSingleton<ITelemetryWebHubClient>(
                s => new TelemetryWebHubClient(
                    this.Configuration.GetValue<Uri>("Telemetry:Url"),
                    s.GetRequiredService<IServiceScopeFactory>()));

            services.AddMemoryCache();

            services.AddScoped<IMachineProvider, MachineProvider>();
            services.AddScoped<IErrorLogProvider, ErrorLogProvider>();
            services.AddScoped<IIOLogProvider, IOLogProvider>();
            services.AddScoped<IMissionLogProvider, MissionLogProvider>();
            services.AddScoped<IScreenShotProvider, ScreenShotProvider>();

            //services.AddTransient<Providers.IMachineProvider, Providers.MachineProvider>();
            //services.AddTransient<Providers.IErrorLogProvider, Providers.ErrorLogProvider>();
            //services.AddTransient<Providers.IIOLogProvider, Providers.IOLogProvider>();
            //services.AddTransient<Providers.IMissionLogProvider, Providers.MissionLogProvider>();
            //services.AddTransient<Providers.IScreenShotProvider, Providers.ScreenShotProvider>();

            var connectionString = this.Configuration.GetConnectionString(ConnectionStringName);
            var schemaVersion = Convert.ToUInt64(this.Configuration.GetConnectionString(SchemaVersionName));

            var config = new RealmConfiguration(connectionString) { SchemaVersion = schemaVersion };
            try
            {
                services.AddTransient(s => Realm.GetInstance(config));
            }
            catch (Realms.Exceptions.RealmFileAccessErrorException exc)
            {
                System.Diagnostics.Debug.WriteLine($"Realm File access error exception. Reason: {exc}");
                return;
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid exception for Realm database");
                return;
            }
            //services.AddTransient(s => Realm.GetInstance(new RealmConfiguration(s.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName))));

            services.AddHostedService<DatabaseCleanupService>();
        }

        #endregion
    }
}
