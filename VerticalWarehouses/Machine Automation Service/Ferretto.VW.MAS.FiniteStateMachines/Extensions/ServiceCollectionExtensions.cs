using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.FiniteStateMachines
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddFiniteStateMachines(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHostedService<FiniteStateMachines>();

            return services;
        }

        #endregion
    }
}
