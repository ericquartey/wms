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

            serviceCollection.AddTransient<IItemsDataService>(s => new ItemsDataService(baseUrl.AbsoluteUri));
            serviceCollection.AddTransient<IMissionsDataService>(s => new MissionsDataService(baseUrl.AbsoluteUri));
            serviceCollection.AddTransient<IItemListsDataService>(s => new ItemListsDataService(baseUrl.AbsoluteUri));

            return serviceCollection;
        }

        #endregion Methods
    }
}
