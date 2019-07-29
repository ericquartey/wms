using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Simulator.Inverter.Interface;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.Simulator.Inverter
{
    internal class InverterService : IInverterService
    {
        #region Fields

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public InverterService() //ILogger<InverterService> logger
        {
            //this.logger = logger;

            //this.logger.LogTrace("1:Subscription Command");
        }

        #endregion

        #region Destructors

        ~InverterService()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        public bool IsStartedSimulator { get; private set; }

        #endregion

        #region Methods

        public void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
        }

        /// <summary>
        /// Start simulator, inizialize socket (???)
        /// </summary>
        /// <returns></returns>
        public async Task ProcessStartSimulatorAsync()
        {
            this.logger.LogTrace("1:ProcessStartSimulator");
            await Task.Delay(100);

            this.IsStartedSimulator = true;
        }

        #endregion
    }
}
