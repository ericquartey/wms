using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.SocketLink
{
    internal sealed class WmsSocketLinkProvider : ISocketLinkSyncProvider
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public WmsSocketLinkProvider(
            IConfiguration configuration,
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        #endregion

        #region Properties

        public bool CanEnableSocketLinkSyncMode => true;    // TODO, must be configurated

        public bool IsWmsAutoSyncEnabled
        {
            get => true;    // TODO, must be configurated
            set => throw new NotImplementedException();
        }

        #endregion

        #region Methods

        public ExtractCommandResponseCode ExtractCommand(int tryNumber, int WarehouseNumber, int ExitBayNumber)
        {
            throw new NotImplementedException();
        }

        public StoreCommandResponseCode StoreCommand(int WarehouseNumber, int BayNumber)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
