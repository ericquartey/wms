using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Scheduler.Core
{
    public static class ServiceCollectionCoreExtensions
    {
        #region Methods

        public static IServiceCollection AddCoreBusinessProviders(
                 this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDataProvider, DataProvider>();
            serviceCollection.AddTransient<ISchedulerRequestProvider, SchedulerRequestProvider>();

            return serviceCollection;
        }

        #endregion
    }
}
