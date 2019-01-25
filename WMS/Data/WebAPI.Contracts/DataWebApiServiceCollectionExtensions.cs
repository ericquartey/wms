using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public static class DataWebApiServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddWebApiServices(
             this IServiceCollection serviceCollection, System.Uri baseUrl)
        {
            serviceCollection.AddTransient<IItemsService>(s => new ItemsService(baseUrl.AbsoluteUri));
            serviceCollection.AddTransient<IMissionsService>(s => new MissionsService(baseUrl.AbsoluteUri));
            serviceCollection.AddTransient<IItemListsService>(s => new ItemListsService(baseUrl.AbsoluteUri));

            return serviceCollection;
        }

        #endregion Methods
    }
}
