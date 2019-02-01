using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public static class WebApiServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddWebApiServices(
             this IServiceCollection serviceCollection, System.Uri baseUrl)
        {
            serviceCollection.AddTransient(s => SchedulerServiceFactory.GetService<IItemsSchedulerService>(baseUrl));
            serviceCollection.AddTransient(s => SchedulerServiceFactory.GetService<IItemListsSchedulerService>(baseUrl));
            serviceCollection.AddTransient(s => SchedulerServiceFactory.GetService<IItemListRowsSchedulerService>(baseUrl));
            serviceCollection.AddTransient(s => SchedulerServiceFactory.GetService<IMissionsSchedulerService>(baseUrl));
            serviceCollection.AddTransient(s => SchedulerServiceFactory.GetService<IBaysSchedulerService>(baseUrl));

            return serviceCollection;
        }

        #endregion Methods
    }
}
