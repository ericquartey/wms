using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public static class DataWebAPIServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddWebAPIServices(
             this IServiceCollection serviceCollection, string baseUrl)
        {
            serviceCollection.AddTransient<IItemsService>(s => new ItemsService(baseUrl));
            serviceCollection.AddTransient<IMissionsService>(s => new MissionsService(baseUrl));
            serviceCollection.AddTransient<IListsService>(s => new ListsService(baseUrl));

            return serviceCollection;
        }

        #endregion Methods
    }
}
