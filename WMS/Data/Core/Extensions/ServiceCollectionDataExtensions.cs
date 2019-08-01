using AutoMapper;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Data.Core.Extensions
{
    public static class ServiceCollectionDataExtensions
    {
        #region Methods

        public static IServiceCollection AddDataServiceProviders(
                         this IServiceCollection services)
        {
            if (services == null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            services.AddTransient<IAbcClassProvider, AbcClassProvider>();
            services.AddTransient<IAisleProvider, AisleProvider>();
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
            services.AddTransient<IItemListRowProvider, ItemListRowProvider>();
            services.AddTransient<IItemAreaProvider, ItemAreaProvider>();
            services.AddTransient<IItemProvider, ItemProvider>();
            services.AddTransient<ILoadingUnitProvider, LoadingUnitProvider>();
            services.AddTransient<ILoadingUnitStatusProvider, LoadingUnitStatusProvider>();
            services.AddTransient<ILoadingUnitTypeProvider, LoadingUnitTypeProvider>();
            services.AddTransient<IMachineProvider, MachineProvider>();
            services.AddTransient<IMaterialStatusProvider, MaterialStatusProvider>();
            services.AddTransient<IMeasureUnitProvider, MeasureUnitProvider>();
            services.AddTransient<IMissionProvider, MissionProvider>();
            services.AddTransient<IPackageTypeProvider, PackageTypeProvider>();
            services.AddTransient<ISchedulerRequestProvider, SchedulerRequestProvider>();
            services.AddTransient<IUserProvider, UserProvider>();
            services.AddTransient<IImageProvider, ImageProvider>();
            services.AddTransient<IGlobalSettingsProvider, GlobalSettingsProvider>();

            services.AddHostedService<MachineLiveDataService>();
            services.AddSingleton<IMachinesLiveDataContext, MachinesLiveDataContext>();

            services.AddSingleton<INotificationService, NotificationService>();

            services.AddAutoMapper(
                typeof(BaseModel<int>),
                typeof(Common.DataModels.IDataModel<int>));
            services.AddAutoMapper(
                typeof(BaseModel<string>),
                typeof(Common.DataModels.IDataModel<string>));

            return services;
        }

        #endregion
    }
}
