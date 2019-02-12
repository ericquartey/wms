using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Scheduler.Core.Extensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
       "Major Code Smell",
       "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
       Justification = "This class register services into container")]
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
