using System;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ferretto.VW.MAS.DataLayer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var dataLayerConfiguration = new DataLayerConfiguration(
                configuration.GetDataLayerPrimaryConnectionString(),
                configuration.GetDataLayerSecondaryConnectionString(),
                configuration.GetDataLayerConfigurationFile());

            services.AddSingleton(dataLayerConfiguration);

            services.AddSingleton<IDbContextRedundancyService<DataLayerContext>>(
                new DbContextRedundancyService<DataLayerContext>(
                   configuration.GetDataLayerPrimaryConnectionString(),
                   configuration.GetDataLayerSecondaryConnectionString()));

            services.AddTransient(p =>
                new DataLayerContext(
                    p.GetRequiredService<IDbContextRedundancyService<DataLayerContext>>().ActiveDbContextOptions,
                    p.GetRequiredService<IDbContextRedundancyService<DataLayerContext>>()));

            services.AddSingleton<IDataLayerService, DataLayerService>();

            services.AddSingleton(p => p.GetService<IDataLayerService>() as IHostedService);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IBayPositionControlDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IBeltBurnishingDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as ICellControlDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IGeneralInfoConfigurationDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IHorizontalAxisDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IHorizontalManualMovementsDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IHorizontalMovementBackwardProfileDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IHorizontalMovementForwardProfileDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as ILoadFirstDrawerDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IOffsetCalibrationDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IPanelControlDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IResolutionCalibrationDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as ISetupNetworkDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as ISetupStatusDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IShutterHeightControlDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IVerticalAxisDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IVerticalManualMovementsDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IWeightControlDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as ICellManagmentDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IConfigurationValueManagmentDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IVertimagConfigurationDataLayer);
            services.AddSingleton(p => p.GetService<IDataLayerService>() as IResolutionConversionDataLayer);

            services.AddTransient<IServicingProvider, ServicingProvider>();
            services.AddTransient<IBaysProvider, BaysProvider>();
            services.AddTransient<ICellsProvider, CellsProvider>();
            services.AddTransient<IErrorsProvider, ErrorsProvider>();
            services.AddTransient<IMachineStatisticsProvider, MachineStatisticsProvider>();
            services.AddTransient<IUsersProvider, UsersProvider>();
            services.AddTransient<IBaysConfigurationProvider, BaysConfigurationProvider>();
            services.AddTransient<ILoadingUnitStatisticsProvider, LoadingUnitStatisticsProvider>();
            services.AddTransient<IBaysConfigurationProvider, BaysConfigurationProvider>();

            return services;
        }

        #endregion
    }
}
