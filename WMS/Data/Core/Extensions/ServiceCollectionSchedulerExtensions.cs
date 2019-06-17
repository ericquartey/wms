using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.WMS.Data.Core.Extensions
{
    public static class ServiceCollectionSchedulerExtensions
    {
        #region Methods

        public static IServiceCollection AddSchedulerServiceProvider<T>(
                this IServiceCollection serviceCollection)
        {
            switch (typeof(T))
            {
                case var serviceType when serviceType == typeof(ICompartmentOperationProvider):

                    serviceCollection.AddTransient<ICompartmentOperationProvider, CompartmentOperationProvider>();

                    break;
            }

            return serviceCollection;
        }

        public static IServiceCollection AddSchedulerServiceProviders(
                         this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IItemListRowExecutionProvider, ItemListRowExecutionProvider>();
            serviceCollection.AddTransient<IItemListExecutionProvider, ItemListExecutionProvider>();
            serviceCollection.AddTransient<IMissionExecutionProvider, MissionExecutionProvider>();
            serviceCollection.AddTransient<ISchedulerRequestExecutionProvider, SchedulerRequestExecutionProvider>();
            serviceCollection.AddTransient<ISchedulerRequestPickProvider, SchedulerRequestPickProvider>();
            serviceCollection.AddTransient<ISchedulerRequestPutProvider, SchedulerRequestPutProvider>();
            serviceCollection.AddTransient<ICompartmentOperationProvider, CompartmentOperationProvider>();
            serviceCollection.AddTransient<IMissionCreationProvider, MissionCreationProvider>();

            serviceCollection.AddHostedService<Services.SchedulerService>();
            serviceCollection.AddTransient<ISchedulerService, Services.SchedulerService>();

            return serviceCollection;
        }

        #endregion
    }
}
