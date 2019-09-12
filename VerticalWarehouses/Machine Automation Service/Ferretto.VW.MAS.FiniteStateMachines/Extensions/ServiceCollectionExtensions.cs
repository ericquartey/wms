using System;
using System.Diagnostics;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
