using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Ferretto.VW.InverterDriver
{
    public class HostedInverterDriverMock : BackgroundService
    {
        #region Methods

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("HostedInverterDriverMock ExecuteAsync");
            return Task.Delay(1);
        }

        #endregion
    }
}
