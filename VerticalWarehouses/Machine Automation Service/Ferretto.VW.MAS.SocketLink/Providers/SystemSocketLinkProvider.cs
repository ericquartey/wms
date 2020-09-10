using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.SocketLink.Models;
using Ferretto.VW.MAS.SocketLink.Providers;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.SocketLink
{
    internal sealed class SystemSocketLinkProvider : ISystemSocketLinkProvider
    {
        #region Fields

        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly PubSubEvent<SocketLinkChangeRequestEventArgs> socketLinkChangeRequestEventArgs;

        #endregion

        #region Constructors

        public SystemSocketLinkProvider(
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            this.socketLinkChangeRequestEventArgs = eventAggregator.GetEvent<PubSubEvent<SocketLinkChangeRequestEventArgs>>();
        }

        #endregion

        #region Properties

        public bool CanEnableSyncMode => this.serviceScopeFactory
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<IWmsSettingsProvider>()
            .SocketLinkIsEnabled;

        bool ISystemSocketLinkProvider.CanEnableSyncMode
        {
            get => this.serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IWmsSettingsProvider>().SocketLinkIsEnabled;
            set
            {
                this.serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IWmsSettingsProvider>().IsTimeSyncEnabled = value;

                this.socketLinkChangeRequestEventArgs.Publish(new SocketLinkChangeRequestEventArgs(value));
            }
        }

        #endregion
    }
}
