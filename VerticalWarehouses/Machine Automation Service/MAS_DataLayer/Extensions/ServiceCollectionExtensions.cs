using System;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.EntityFrameworkCore;
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

            services.AddDbContext<DataLayerContext>(
               options => options.UseSqlite(configuration.GetDataLayerPrimaryConnectionString()),
               ServiceLifetime.Singleton);

            services.AddSingleton(
                new DataLayerConfiguration(
                configuration.GetDataLayerSecondaryConnectionString(),
                configuration.GetDataLayerConfigurationFile()));

            services.AddSingleton<IDataLayer, DataLayerService>();

            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IHostedService);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IBayPositionControlDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IBeltBurnishingDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as ICellControlDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IGeneralInfoDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IHorizontalAxis);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IHorizontalManualMovements);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IHorizontalMovementBackwardProfile);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IHorizontalMovementForwardProfile);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as ILoadFirstDrawer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IOffsetCalibration);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IPanelControl);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IResolutionCalibration);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as ISetupNetwork);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as ISetupStatus);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IShutterHeightControl);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IVerticalAxis);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IVerticalManualMovements);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IWeightControl);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as ICellManagmentDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IConfigurationValueManagmentDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IRuntimeValueManagmentDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IVertimagConfiguration);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IErrorStatisticsDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IMachineStatisticsDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as ILoadingUnitStatistics);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IResolutionConversion);

            services.AddScoped<IServicingProvider, ServicingProvider>();

            return services;
        }

        #endregion
    }
}
