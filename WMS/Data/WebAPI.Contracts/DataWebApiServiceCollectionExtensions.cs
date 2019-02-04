using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
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
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IMissionsDataService>(baseUrl));
            serviceCollection.AddTransient(s => DataServiceFactory.GetService<IItemListsDataService>(baseUrl));

            return serviceCollection;
        }

        #endregion
    }
}
