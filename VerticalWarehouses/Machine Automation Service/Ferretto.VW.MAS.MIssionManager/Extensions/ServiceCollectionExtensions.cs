using System;
using System.Net.Http;
using Ferretto.VW.MAS.DataLayer;
//using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.MAS.MissionManager
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddAdapterWebServices(this IServiceCollection serviceCollection, Func<IServiceProvider, Uri> baseUrlResolver, Func<IServiceProvider, int> machineIdResolver)
        {
            serviceCollection.AddScoped(s =>
            {
                var client = new HttpClient();

                client.DefaultRequestHeaders.Add(
                    "Machine-Id",
                    machineIdResolver(s).ToString(System.Globalization.CultureInfo.InvariantCulture));

                return client;
            });

            serviceCollection.AddScoped<IMachineLoadingUnitsAdapterWebService>(
                s => new MachineLoadingUnitsAdapterWebService(baseUrlResolver(s).ToString(), s.GetService<HttpClient>()));

            serviceCollection.AddScoped<IMachineUsersAdapterWebService>(
                s => new MachineUsersAdapterWebService(baseUrlResolver(s).ToString(), s.GetService<HttpClient>()));

            //serviceCollection.AddScoped<IMachineMachinesAdapterWebService>(
            //    s => new MachineMachinesAdapterWebService(baseUrlResolver(s).ToString(), s.GetService<HttpClient>()));

            return serviceCollection;
        }

        public static IServiceCollection AddAdapterWebServices(this IServiceCollection serviceCollection, Uri baseUrl, int machineId)
        {
            return AddAdapterWebServices(serviceCollection, s => baseUrl, s => machineId);
        }

        public static IServiceCollection AddMissionManager(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddHostedService<MissionSchedulingService>()
                .AddScoped<IMissionSchedulingProvider, MissionSchedulingProvider>();

            return services;
        }

        public static IServiceCollection AddWmsMissionManager(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddHostedService<WmsMissionProxyService>()
                .AddScoped<IMissionOperationsProvider, MissionOperationsProvider>();

            return services;
        }

        #endregion
    }
}
