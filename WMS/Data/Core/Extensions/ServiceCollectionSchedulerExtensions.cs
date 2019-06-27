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
            serviceCollection.AddTransient<ICompartmentOperationProvider, CompartmentOperationProvider>();
            serviceCollection.AddTransient<IItemListExecutionProvider, ItemListExecutionProvider>();
            serviceCollection.AddTransient<IItemListRowExecutionProvider, ItemListRowExecutionProvider>();
            serviceCollection.AddTransient<IMissionLoadingUnitProvider, MissionLoadingUnitProvider>();
            serviceCollection.AddTransient<IMissionOperationCreationProvider, MissionOperationCreationProvider>();
            serviceCollection.AddTransient<IMissionOperationProvider, MissionOperationProvider>();
            serviceCollection.AddTransient<ISchedulerRequestExecutionProvider, SchedulerRequestExecutionProvider>();
            serviceCollection.AddTransient<ISchedulerRequestPickProvider, SchedulerRequestPickProvider>();
            serviceCollection.AddTransient<ISchedulerRequestPutProvider, SchedulerRequestPutProvider>();

            serviceCollection.AddTransient<ISchedulerService, Services.SchedulerService>();
            serviceCollection.AddTransient<IItemListSchedulerService, Services.SchedulerService>();
            serviceCollection.AddTransient<IItemSchedulerService, Services.SchedulerService>();
            serviceCollection.AddTransient<IMissionSchedulerService, Services.SchedulerService>();
            serviceCollection.AddHostedService<Services.SchedulerService>();

            return serviceCollection;
        }

        #endregion
    }
}
