using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Data.Core.Extensions
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
            serviceCollection.AddTransient<IItemListRowExecutionProvider, ItemListRowExecutionProvider>();
            serviceCollection.AddTransient<IItemListExecutionProvider, ItemListExecutionProvider>();
            serviceCollection.AddTransient<IMissionExecutionProvider, MissionExecutionProvider>();
            serviceCollection.AddTransient<ISchedulerRequestProvider, SchedulerRequestProvider>();
            serviceCollection.AddTransient<ICompartmentExecutionProvider, CompartmentExecutionProvider>();
            serviceCollection.AddTransient<IMissionCreationProvider, MissionCreationProvider>();

            serviceCollection.AddHostedService<Services.SchedulerService>();
            serviceCollection.AddTransient<ISchedulerService, Services.SchedulerService>();

            return serviceCollection;
        }

        #endregion
    }
}
