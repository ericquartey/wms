using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.AutomationService
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

            //serviceCollection.AddSingleton<IMachineLoadingUnitsAdapterWebService>(
            //    s => new MachineLoadingUnitsAdapterWebService(baseUrlResolver(s).ToString(), s.GetService<HttpClient>()));

            return serviceCollection;
        }

        public static IServiceCollection AddAdapterWebServices(this IServiceCollection serviceCollection, Uri baseUrl, int machineId)
        {
            return AddAdapterWebServices(serviceCollection, s => baseUrl, s => machineId);
        }

        #endregion
    }
}
