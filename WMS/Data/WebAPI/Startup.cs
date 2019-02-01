using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#if DEBUG
using NSwag.AspNetCore;
#endif

namespace Ferretto.WMS.Data.WebAPI
{
    public class Startup
    {
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

        /// <summary>
        ///  This method gets called by the runtime.
        ///  Use this method to configure the HTTP request pipeline.
        /// </summary>
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
#if DEBUG
                app.UseSwaggerUi3WithApiExplorer(settings =>
                {
                    settings.PostProcess = document =>
                    {
                        var assembly = typeof(Startup).Assembly;
                        var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

                        document.Info.Version = versionInfo.FileVersion;
                        document.Info.Title = "WMS Data API";
                        document.Info.Description = "REST API for the WMS Data Service";
                    };
                    settings.GeneratorSettings.DefaultPropertyNameHandling =
                        NJsonSchema.PropertyNameHandling.CamelCase;

                    settings.GeneratorSettings.DefaultEnumHandling = NJsonSchema.EnumHandling.String;
                });
#endif
            }
            else if (env.IsProduction())
            {
                app.UseHsts();

                app.UseHttpsRedirection();
            }

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var connectionString = this.Configuration.GetConnectionString("WmsConnectionString");
            services.AddDbContext<DatabaseContext>(
                options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Ferretto.Common.EF")));

            services.AddTransient<IAbcClassProvider, AbcClassProvider>();
            services.AddTransient<IAreaProvider, AreaProvider>();
            services.AddTransient<IBayProvider, BayProvider>();
            services.AddTransient<ICellProvider, CellProvider>();
            services.AddTransient<ICellStatusProvider, CellStatusProvider>();
            services.AddTransient<ICellTypeProvider, CellTypeProvider>();
            services.AddTransient<IItemListsProvider, ItemListsProvider>();
            services.AddTransient<IItemProvider, ItemProvider>();
            services.AddTransient<IMissionProvider, MissionProvider>();

            services.AddMemoryCache();

            services.AddSignalR();
        }

        #endregion Methods
    }
}
