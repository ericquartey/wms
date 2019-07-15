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

            serviceCollection.AddSingleton(new HttpClient());

            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IAbcClassesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IAislesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IAreasDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IBaysDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellPositionsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellStatusesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellTypesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICellsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICompartmentStatusesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICompartmentTypesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ICompartmentsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IImagesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemCategoriesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemListRowsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemListsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemListsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ILoadingUnitStatusesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ILoadingUnitTypesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ILoadingUnitsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMachinesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMaterialStatusesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMeasureUnitsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMissionOperationsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMissionsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IPackageTypesDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ISchedulerRequestsDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IUsersDataService>(baseUrl, s.GetService<HttpClient>()));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<ILocalizationDataService>(baseUrl, s.GetService<HttpClient>()));

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
