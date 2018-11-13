using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public static class WebAPIServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddWebAPIClients(
             this IServiceCollection serviceCollection, string baseUrl)
        {
            serviceCollection.AddTransient<IItemsClient>(s => new ItemsClient(baseUrl));
            serviceCollection.AddTransient<IMissionsClient>(s => new MissionsClient(baseUrl));
            serviceCollection.AddTransient<IBaysClient>(s => new BaysClient(baseUrl));

            return serviceCollection;
        }

        #endregion Methods
    }
}
