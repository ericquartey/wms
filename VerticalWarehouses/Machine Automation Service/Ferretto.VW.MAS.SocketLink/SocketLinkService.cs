using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Prism.Events;

namespace Ferretto.VW.MAS.SocketLink
{
    internal sealed class SocketLinkService : ISocketLinkService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public SocketLinkService(
            IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Methods

        public Task StartAsync()
        {
            this.logger.Info("Starting Socket Link Service.");
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            this.logger.Info("Stopping Socket Link Service.");
            throw new NotImplementedException();
        }

        #endregion
    }
}
