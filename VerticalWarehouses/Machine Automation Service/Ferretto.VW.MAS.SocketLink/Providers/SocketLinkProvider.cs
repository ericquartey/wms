using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS.DataLayer;
using Prism.Events;

namespace Ferretto.VW.MAS.SocketLink
{
    public class SocketLinkProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public SocketLinkProvider(IEventAggregator eventAggregator, IBaysDataProvider baysDataProvider)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));
            this.baysDataProvider = baysDataProvider ?? throw new System.ArgumentNullException(nameof(baysDataProvider));
        }

        #endregion
    }
}
