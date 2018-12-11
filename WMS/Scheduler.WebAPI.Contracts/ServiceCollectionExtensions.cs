using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public static class WebAPIServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddWebAPIServices(
             this IServiceCollection serviceCollection, string baseUrl)
        {
            serviceCollection.AddTransient<IItemsService>(s => new ItemsService(baseUrl));
            serviceCollection.AddTransient<IItemListsService>(s => new ItemListsService(baseUrl));
            serviceCollection.AddTransient<IMissionsService>(s => new MissionsService(baseUrl));
            serviceCollection.AddTransient<IBaysService>(s => new BaysService(baseUrl));

            return serviceCollection;
        }

        #endregion Methods
    }
}
