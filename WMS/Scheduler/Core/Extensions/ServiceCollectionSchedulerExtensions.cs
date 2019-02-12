using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Scheduler.Core.Extensions
{
    public static class ServiceCollectionSchedulerExtensions
    {
        #region Methods

        public static IServiceCollection AddSchedulerServiceProviders(
                 this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDataProvider, DataProvider>();
            serviceCollection.AddTransient<ISchedulerRequestProvider, SchedulerRequestProvider>();

            return serviceCollection;
        }

        #endregion
    }
}
