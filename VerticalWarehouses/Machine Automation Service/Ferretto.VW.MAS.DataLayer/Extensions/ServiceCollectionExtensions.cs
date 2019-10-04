using System;
using System.Diagnostics;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddDataLayer(
                    this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IDbContextRedundancyService<DataLayerContext>, DbContextRedundancyService<DataLayerContext>>();

            services.AddScoped(p =>
                new DataLayerContext(
                   isActiveChannel: true,
                   p.GetRequiredService<IDbContextRedundancyService<DataLayerContext>>()));

            services
                .AddSingleton<IDataLayerService, DataLayerService>();

            services
                .AddSingleton(p => p.GetService<IDataLayerService>() as IBayPositionControlDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IBeltBurnishingDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as ICellControlDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IConfigurationValueManagmentDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IHorizontalManualMovementsDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IHostedService)
                .AddSingleton(p => p.GetService<IDataLayerService>() as ILoadFirstDrawerDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IOffsetCalibrationDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IPanelControlDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IResolutionCalibrationDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IShutterHeightControlDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IShutterManualMovementsDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IVerticalManualMovementsDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IWeightControlDataLayer);

            services
                .AddScoped<IBaysProvider, BaysProvider>()
                .AddScoped<ICellPanelsProvider, CellPanelsProvider>()
                .AddScoped<ICellsProvider, CellsProvider>()
                .AddScoped<IDigitalDevicesDataProvider, DigitalDevicesDataProvider>()
                .AddScoped<IElevatorDataProvider, ElevatorDataProvider>()
                .AddScoped<IElevatorWeightCheckProcedureProvider, ElevatorWeightCheckProcedureProvider>()
                .AddScoped<IErrorsProvider, ErrorsProvider>()
                .AddScoped<ILoadingUnitsProvider, LoadingUnitsProvider>()
                .AddScoped<ILogEntriesProvider, LogEntriesProvider>()
                .AddScoped<IMachineProvider, MachineProvider>()
                .AddScoped<IServicingProvider, ServicingProvider>()
                .AddScoped<ISetupStatusProvider, SetupStatusProvider>()
                .AddScoped<IShutterTestParametersProvider, ShutterTestParametersProvider>()
                .AddScoped<ITorqueCurrentMeasurementsDataProvider, TorqueCurrentMeasurementsDataProvider>()
                .AddScoped<IUsersProvider, UsersProvider>();

            services
                .AddSingleton<IVerticalOriginVolatileSetupStatusProvider, VerticalOriginVolatileSetupStatusProvider>();

            return services;
        }

        public static IApplicationBuilder UseDataLayer(this IApplicationBuilder app)
        {
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
