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
                .AddSingleton<IDbContextRedundancyService<DataLayerContext>, DbContextRedundancyService<DataLayerContext>>()
                .AddScoped(p =>
                    new DataLayerContext(
                       isActiveChannel: true,
                       p.GetRequiredService<IDbContextRedundancyService<DataLayerContext>>()));

            services
                .AddTransient<IBaysDataProvider, BaysDataProvider>()
                .AddTransient<ICellPanelsProvider, CellPanelsProvider>()
                .AddTransient<ICellsProvider, CellsProvider>()
                .AddTransient<IDigitalDevicesDataProvider, DigitalDevicesDataProvider>()
                .AddTransient<IElevatorDataProvider, ElevatorDataProvider>()
                .AddTransient<IElevatorWeightCheckProcedureProvider, ElevatorWeightCheckProcedureProvider>()
                .AddTransient<IErrorsProvider, ErrorsProvider>()
                .AddTransient<ILoadingUnitsProvider, LoadingUnitsProvider>()
                .AddTransient<ILogEntriesProvider, LogEntriesProvider>()
                .AddTransient<IServicingProvider, ServicingProvider>()
                .AddTransient<ISetupStatusProvider, SetupStatusProvider>()
                .AddTransient<ISetupProceduresDataProvider, SetupProceduresDataProvider>()
                .AddTransient<ITorqueCurrentMeasurementsDataProvider, TorqueCurrentMeasurementsDataProvider>()
                .AddTransient<IUsersProvider, UsersProvider>();

            services
                .AddSingleton<IMachineProvider, MachineProvider>()
                .AddSingleton<IVerticalOriginVolatileSetupStatusProvider, VerticalOriginVolatileSetupStatusProvider>()
                .AddSingleton<IMachineModeVolatileDataProvider, MachineModeVolatileDataProvider>()
                .AddSingleton<IElevatorVolatileDataProvider, ElevatorVolatileDataProvider>()
                .AddSingleton<IBayChainVolatileDataProvider, BayChainVolatileDataProvider>()
                .AddSingleton<IMachineMissionsProvider, MachineMissionsProvider>();

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
