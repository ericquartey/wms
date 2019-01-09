using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.Common.BusinessProviders
{
    public static class ServiceCollectionProvidersExtensions
    {
        #region Methods

        public static IServiceCollection AddBusinessProviders(
             this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IAreaProvider, AreaProvider>();
            serviceCollection.AddTransient<IBayProvider, BayProvider>();
            serviceCollection.AddTransient<ICellProvider, CellProvider>();
            serviceCollection.AddTransient<ICompartmentProvider, CompartmentProvider>();
            serviceCollection.AddTransient<IItemProvider, ItemProvider>();
            serviceCollection.AddTransient<IItemListProvider, ItemListProvider>();
            serviceCollection.AddTransient<IItemListRowProvider, ItemListRowProvider>();
            serviceCollection.AddTransient<ILoadingUnitProvider, LoadingUnitProvider>();
            serviceCollection.AddTransient<IMachineProvider, MachineProvider>();
            serviceCollection.AddTransient<IMissionProvider, MissionProvider>();
            serviceCollection.AddTransient<EnumerationProvider, EnumerationProvider>();

            return serviceCollection;
        }

        #endregion Methods
    }
}
