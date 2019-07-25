using System;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Utilities;
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

            var dataLayerConfiguration = new DataLayerConfiguration(
                configuration.GetDataLayerPrimaryConnectionString(),
                configuration.GetDataLayerSecondaryConnectionString(),
                configuration.GetDataLayerConfigurationFile());

            services.AddDbContext<DataLayerContext>(
               options => options.UseSqlite(configuration.GetDataLayerPrimaryConnectionString()));

            services.AddSingleton(dataLayerConfiguration);

            services.AddSingleton<IDataLayer, DataLayerService>();

            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IHostedService);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IBayPositionControlDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IBeltBurnishingDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as ICellControlDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IGeneralInfoConfigurationDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IHorizontalAxisDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IHorizontalManualMovementsDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IHorizontalMovementBackwardProfileDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IHorizontalMovementForwardProfileDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as ILoadFirstDrawerDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IOffsetCalibrationDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IPanelControlDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IResolutionCalibrationDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as ISetupNetworkDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as ISetupStatusDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IShutterHeightControlDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IVerticalAxisDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IVerticalManualMovementsDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IWeightControlDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as ICellManagmentDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IConfigurationValueManagmentDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IVertimagConfigurationDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IErrorStatisticsProvider);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IMachineStatisticsDataLayer);
            services.AddSingleton(provider => provider.GetService<IDataLayer>() as IResolutionConversionDataLayer);

            services.AddTransient<IServicingProvider, ServicingProvider>();
            services.AddTransient<IBaysProvider, BaysProvider>();
            services.AddTransient<IBaysConfigurationProvider, BaysConfigurationProvider>();
            services.AddTransient<ILoadingUnitStatisticsProvider, LoadingUnitStatisticsProvider>();
            services.AddTransient<IBaysConfigurationProvider, BaysConfigurationProvider>();

            return services;
        }

        #endregion
    }
}
