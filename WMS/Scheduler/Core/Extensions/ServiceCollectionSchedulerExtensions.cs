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
            serviceCollection.AddTransient<ICompartmentSchedulerProvider, CompartmentSchedulerProvider>();
            serviceCollection.AddTransient<IItemListRowSchedulerProvider, ItemListRowSchedulerProvider>();
            serviceCollection.AddTransient<IItemListSchedulerProvider, ItemListSchedulerProvider>();
            serviceCollection.AddTransient<IItemSchedulerProvider, ItemSchedulerProvider>();
            serviceCollection.AddTransient<IMissionSchedulerProvider, MissionSchedulerProvider>();
            serviceCollection.AddTransient<ISchedulerRequestProvider, SchedulerRequestProvider>();
            serviceCollection.AddTransient<ILoadingUnitSchedulerProvider, LoadingUnitSchedulerProvider>();

            serviceCollection.AddHostedService<Services.SchedulerService>();
            serviceCollection.AddTransient<ISchedulerService, Services.SchedulerService>();

            return serviceCollection;
        }

        #endregion
    }
}
