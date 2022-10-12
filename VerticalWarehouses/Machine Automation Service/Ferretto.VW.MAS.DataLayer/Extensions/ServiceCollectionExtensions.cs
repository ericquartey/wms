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
                .AddScoped<IAccessoriesDataProvider, AccessoriesDataProvider>()
                .AddScoped<IBaysDataProvider, BaysDataProvider>()
                .AddScoped<ICellPanelsProvider, CellPanelsProvider>()
                .AddScoped<ICellsProvider, CellsProvider>()
                .AddScoped<IDigitalDevicesDataProvider, DigitalDevicesDataProvider>()
                .AddScoped<IElevatorDataProvider, ElevatorDataProvider>()
                .AddScoped<IElevatorWeightCheckProcedureProvider, ElevatorWeightCheckProcedureProvider>()
                .AddScoped<IErrorsProvider, ErrorsProvider>()
                .AddScoped<ILoadingUnitsDataProvider, LoadingUnitsDataProvider>()
                .AddScoped<ILogEntriesProvider, LogEntriesProvider>()
                .AddScoped<IServicingProvider, ServicingProvider>()
                .AddScoped<IMissionsDataProvider, MissionsDataProvider>()
                .AddScoped<ISetupStatusProvider, SetupStatusProvider>()
                .AddScoped<ISetupProceduresDataProvider, SetupProceduresDataProvider>()
                .AddScoped<ITorqueCurrentMeasurementsDataProvider, TorqueCurrentMeasurementsDataProvider>()
                .AddScoped<IWmsSettingsProvider, WmsSettingsProvider>()
                .AddScoped<IUsersProvider, UsersProvider>()
                .AddScoped<IMachineMissionsProvider, MachineMissionsProvider>()
                .AddScoped<IMachineProvider, MachineProvider>()
                .AddScoped<IStatisticsDataProvider, StatisticsDataProvider>()
                .AddScoped<IAutoCompactingSettingsProvider, AutoCompactingSettingsProvider>()
                .AddScoped<IRotationClassScheduleProvider, RotationClassScheduleProvider>()
                .AddScoped<ILogoutSettingsProvider, LogoutSettingsProvider>();

            services
                .AddSingleton<IVerticalOriginVolatileSetupStatusProvider, VerticalOriginVolatileSetupStatusProvider>()
                .AddSingleton<IMachineMissionsVolatileProvider, MachineMissionsVolatileProvider>()
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
            var machineVolatile = app.ApplicationServices.GetRequiredService<IMachineVolatileDataProvider>();

            listener.SubscribeWithAdapter(new CommandListener<DataLayerContext>(redundancyService, logger, machineVolatile));

            return app;
        }

        #endregion
    }
}
