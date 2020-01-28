﻿using System;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.InverterDriver
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddInverterDriver(
            this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHostedService<InverterDriverService>();

            services.AddTransient<IInvertersProvider, InvertersProvider>();

            services.AddSingleton(s =>
            {
                var configuration = s.GetRequiredService<IConfiguration>();

                return configuration.UseInverterDriverMock()
                    ? new SocketTransportMock() as ISocketTransport
                    : new SocketTransport(configuration) as ISocketTransport;
            });

            return services;
        }

        #endregion
    }
}
