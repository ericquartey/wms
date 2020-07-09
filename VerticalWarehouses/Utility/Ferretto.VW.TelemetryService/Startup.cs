using System;
using System.Collections.Generic;
using Ferretto.VW.TelemetryService.Provider;
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

            app.ApplicationServices.GetRequiredService<ITelemetryWebHubClient>()
                .ConnectAsync();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = null;
            })
            /*.AddMessagePackProtocol(options =>
            {
                options.FormatterResolvers = new List<MessagePack.IFormatterResolver>()
                 {
                     MessagePack.Resolvers.ContractlessStandardResolver.Instance,
                     MessagePack.Resolvers.StandardResolver.Instance
                 };
            })*/;

            services.AddSingleton<ITelemetryWebHubClient>(s => new TelemetryWebHubClient(this.Configuration.GetValue<Uri>("Telemetry:Url")));

            services.AddTransient<IMachineProvider, MachineProvider>();

            services.AddTransient<Realm>(s => Realm.GetInstance(new RealmConfiguration(s.GetRequiredService<IConfiguration>().GetConnectionString("Database"))));
        }

        #endregion
    }
}
