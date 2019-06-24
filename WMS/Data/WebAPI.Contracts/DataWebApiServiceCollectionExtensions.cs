using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public static class DataWebApiServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddDataHub(
            this IServiceCollection serviceCollection, System.Uri baseUrl)
        {
            return serviceCollection
                .AddSingleton<IDataHubClient>(new DataHubClient(baseUrl));
        }

        public static IServiceCollection AddWebApiServices(
            this IServiceCollection serviceCollection, System.Uri baseUrl)
        {
            if (serviceCollection == null)
            {
                throw new System.ArgumentNullException(nameof(serviceCollection));
            }

            var httpClient = new HttpClient();
            serviceCollection.AddSingleton(httpClient);

            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IAbcClassesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IAislesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IAreasDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IBaysDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellPositionsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellStatusesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellTypesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICompartmentStatusesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICompartmentTypesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICompartmentsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IImagesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemCategoriesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemListRowsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemListsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemListsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ILoadingUnitStatusesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ILoadingUnitTypesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ILoadingUnitsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMachinesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMaterialStatusesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMeasureUnitsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMissionOperationsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMissionsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IPackageTypesDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ISchedulerRequestsDataService>(baseUrl, httpClient));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IUsersDataService>(baseUrl, httpClient));

            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ILocalizationDataService>(baseUrl, httpClient));

            return serviceCollection;
        }

        public static IApplicationBuilder UseDataHub(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            var hubClient = builder.ApplicationServices.GetService<IDataHubClient>();
            hubClient.ConnectAsync();

            return builder;
        }

        #endregion
    }
}
