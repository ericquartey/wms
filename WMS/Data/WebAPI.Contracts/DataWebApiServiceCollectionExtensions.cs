using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
       "Major Code Smell",
       "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
       Justification = "This class register services into container")]
    public static class DataWebApiServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddWebApiServices(
             this IServiceCollection serviceCollection, System.Uri baseUrl)
        {
            if (serviceCollection == null)
            {
                throw new System.ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IBaysDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMissionsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ISchedulerRequestsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemListsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemListRowsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IAreasDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IAislesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemListsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ILoadingUnitsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMachinesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICompartmentsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IUsersDataService>(baseUrl));

            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemCompartmentTypesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IAbcClassesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellPositionsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellStatusesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellTypesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICompartmentStatusesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICompartmentTypesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemCategoriesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ILoadingUnitStatusesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ILoadingUnitTypesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMaterialStatusesDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMeasureUnitsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IPackageTypesDataService>(baseUrl));

            return serviceCollection;
        }

        #endregion
    }
}
