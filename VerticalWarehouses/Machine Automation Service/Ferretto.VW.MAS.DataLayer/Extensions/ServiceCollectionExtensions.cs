using System;
using System.Diagnostics;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddDataLayer(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddMemoryCache();

            services
                .AddSingleton<IDataLayerService, DataLayerService>()
                .AddSingleton(p => p.GetService<IDataLayerService>() as IHostedService)
                .AddSingleton<IDbContextRedundancyService<DataLayerContext>, DbContextRedundancyService<DataLayerContext>>();

            services.AddEntityFrameworkSqlite();
            services.AddDbContext<DataLayerContext>();

            services
                .AddTransient<IBaysDataProvider, BaysDataProvider>()
                .AddTransient<ICellPanelsProvider, CellPanelsProvider>()
                .AddTransient<ICellsProvider, CellsProvider>()
                .AddTransient<IDigitalDevicesDataProvider, DigitalDevicesDataProvider>()
                .AddTransient<IElevatorDataProvider, ElevatorDataProvider>()
                .AddTransient<IElevatorWeightCheckProcedureProvider, ElevatorWeightCheckProcedureProvider>()
                .AddTransient<IErrorsProvider, ErrorsProvider>()
                .AddTransient<ILoadingUnitsDataProvider, LoadingUnitsDataProvider>()
                .AddTransient<ILogEntriesProvider, LogEntriesProvider>()
                .AddTransient<IServicingProvider, ServicingProvider>()
                .AddTransient<IMissionsDataProvider, MissionsDataProvider>()
                .AddTransient<ISetupStatusProvider, SetupStatusProvider>()
                .AddTransient<ISetupProceduresDataProvider, SetupProceduresDataProvider>()
                .AddTransient<ITorqueCurrentMeasurementsDataProvider, TorqueCurrentMeasurementsDataProvider>()
                .AddTransient<IWmsSettingsProvider, WmsSettingsProvider>()
                .AddTransient<IUsersProvider, UsersProvider>()
                .AddTransient<IMachineMissionsProvider, MachineMissionsProvider>()
                .AddTransient<IMachineProvider, MachineProvider>();

            services
                .AddSingleton<IVerticalOriginVolatileSetupStatusProvider, VerticalOriginVolatileSetupStatusProvider>()
                .AddSingleton<IMachineVolatileDataProvider, MachineVolatileDataProvider>();

            return services;
        }

        public static IApplicationBuilder UseDataLayer(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var dataContext = app.ApplicationServices.GetRequiredService<DataLayerContext>();

            var listener = dataContext.GetService<DiagnosticSource>() as DiagnosticListener;

            var redundancyService = app.ApplicationServices.GetRequiredService<IDbContextRedundancyService<DataLayerContext>>();
            var logger = app.ApplicationServices.GetRequiredService<ILogger<DataLayerContext>>();

            listener.SubscribeWithAdapter(new CommandListener<DataLayerContext>(redundancyService, logger));

            return app;
        }

        #endregion
    }
}
