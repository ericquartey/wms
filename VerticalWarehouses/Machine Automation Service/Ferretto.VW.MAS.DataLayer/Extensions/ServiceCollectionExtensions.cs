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

            services.AddTransient(p =>
                new DataLayerContext(
                   isActiveChannel: true,
                   p.GetRequiredService<IDbContextRedundancyService<DataLayerContext>>()));

            services.AddSingleton<IDataLayerService, DataLayerService>();

            services
                .AddSingleton(p => p.GetService<IDataLayerService>() as IHostedService)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IBayPositionControlDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IBeltBurnishingDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as ICellControlDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IGeneralInfoConfigurationDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IHorizontalAxisDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IHorizontalManualMovementsDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IHorizontalMovementShorterProfileDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IHorizontalMovementLongerProfileDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as ILoadFirstDrawerDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IOffsetCalibrationDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IPanelControlDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IResolutionCalibrationDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as ISetupNetworkDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IShutterHeightControlDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IShutterManualMovementsDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IVerticalAxisDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IVerticalManualMovementsDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IWeightControlDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as ICellManagmentDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IConfigurationValueManagmentDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IVertimagConfigurationDataLayer)
                .AddSingleton(p => p.GetService<IDataLayerService>() as IResolutionConversionDataLayer);

            services
                .AddTransient<IServicingProvider, ServicingProvider>()
                .AddTransient<IBaysProvider, BaysProvider>()
                .AddTransient<ICellsProvider, CellsProvider>()
                .AddTransient<ICellPanelsProvider, CellPanelsProvider>()
                .AddTransient<IErrorsProvider, ErrorsProvider>()
                .AddTransient<IMachineStatisticsProvider, MachineStatisticsProvider>()
                .AddTransient<IUsersProvider, UsersProvider>()
                .AddTransient<ISetupStatusProvider, SetupStatusProvider>()
                .AddTransient<IBaysConfigurationProvider, BaysConfigurationProvider>()
                .AddTransient<ILoadingUnitsProvider, LoadingUnitsProvider>()
                .AddTransient<IBaysConfigurationProvider, BaysConfigurationProvider>()
                .AddTransient<IShutterTestParametersProvider, ShutterTestParametersProvider>()
                .AddTransient<IElevatorWeightCheckProcedureProvider, ElevatorWeightCheckProcedureProvider>()
                .AddTransient<IElevatorProvider, ElevatorProvider>()
                .AddTransient<IMachineConfigurationProvider, MachineConfigurationProvider>();

            services.AddSingleton<IVerticalOriginVolatileSetupStatusProvider, VerticalOriginVolatileSetupStatusProvider>();

            return services;
        }

        public static IApplicationBuilder UseDataLayer(this IApplicationBuilder app)
        {
            var dataContext = app.ApplicationServices.GetRequiredService<DataLayerContext>();

            var listener = dataContext.GetService<DiagnosticSource>() as DiagnosticListener;

            var redundancyService = app.ApplicationServices.GetRequiredService<IDbContextRedundancyService<DataLayerContext>>();

            listener.SubscribeWithAdapter(new CommandListener<DataLayerContext>(redundancyService));

            return app;
        }

        #endregion
    }
}
