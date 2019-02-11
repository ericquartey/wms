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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This class register services into container")]
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
            services.AddTransient<ICellPositionProvider, CellPositionProvider>();
            services.AddTransient<ICellProvider, CellProvider>();
            services.AddTransient<ICellStatusProvider, CellStatusProvider>();
            services.AddTransient<ICellTypeProvider, CellTypeProvider>();
            services.AddTransient<ICompartmentProvider, CompartmentProvider>();
            services.AddTransient<ICompartmentStatusProvider, CompartmentStatusProvider>();
            services.AddTransient<ICompartmentTypeProvider, CompartmentTypeProvider>();
            services.AddTransient<IItemCategoryProvider, ItemCategoryProvider>();
            services.AddTransient<IItemCompartmentTypeProvider, ItemCompartmentTypeProvider>();
            services.AddTransient<IItemListProvider, ItemListProvider>();
            services.AddTransient<IItemProvider, ItemProvider>();
            services.AddTransient<ILoadingUnitStatusProvider, LoadingUnitStatusProvider>();
            services.AddTransient<ILoadingUnitTypeProvider, LoadingUnitTypeProvider>();
            services.AddTransient<IMachineProvider, MachineProvider>();
            services.AddTransient<IMaterialStatusProvider, MaterialStatusProvider>();
            services.AddTransient<IMeasureUnitProvider, MeasureUnitProvider>();
            services.AddTransient<IMissionProvider, MissionProvider>();
            services.AddTransient<IPackageTypeProvider, PackageTypeProvider>();

            services.AddMemoryCache();

            services.AddSignalR();
        }

        #endregion
    }
}
