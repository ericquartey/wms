using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.InverterDriver
{
    public class HostedInverterDriverMock : BackgroundService
    {
        #region Fields

        private readonly ILogger<HostedInverterDriverMock> logger;

        #endregion

        #region Constructors

        public HostedInverterDriverMock(ILogger<HostedInverterDriverMock> logger)
        {
            this.logger = logger;
        }

        #endregion

        #region Methods

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.Log(LogLevel.Debug, "HostedInverterDriverMock ExecuteAsync", null);
            return Task.Delay(1);
        }

        #endregion
    }
}
