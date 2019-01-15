using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public static class WebApiServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddWebApiServices(
             this IServiceCollection serviceCollection, System.Uri baseUrl)
        {
            serviceCollection.AddTransient<IItemsService>(s => new ItemsService(baseUrl.AbsoluteUri));
            serviceCollection.AddTransient<IItemListsService>(s => new ItemListsService(baseUrl.AbsoluteUri));
            serviceCollection.AddTransient<IItemListRowsService>(s => new ItemListRowsService(baseUrl.AbsoluteUri));
            serviceCollection.AddTransient<IMissionsService>(s => new MissionsService(baseUrl.AbsoluteUri));
            serviceCollection.AddTransient<IBaysService>(s => new BaysService(baseUrl.AbsoluteUri));

            return serviceCollection;
        }

        #endregion Methods
    }
}
