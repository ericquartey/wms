using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Data.Core.Extensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
           "Major Code Smell",
           "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
           Justification = "This class register services into container")]
    public static class ServiceCollectionDataExtensions
    {
        #region Methods

        public static IServiceCollection AddDataServiceProviders(
                         this IServiceCollection services)
        {
            if (services != null)
            {
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
                services.AddTransient<IImageFileProvider, ImageFileProvider>();
            }

            return services;
        }

        #endregion
    }
}
