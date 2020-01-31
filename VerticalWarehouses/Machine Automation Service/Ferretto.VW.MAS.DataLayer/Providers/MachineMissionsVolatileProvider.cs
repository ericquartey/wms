using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.Utils.Missions;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MachineMissionsVolatileProvider : IMachineMissionsVolatileProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public MachineMissionsVolatileProvider(
            IServiceScopeFactory serviceScopeFactory,
            IEventAggregator eventAggregator)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));

            this.MachineMissions = new List<IMission>();
        }

        #endregion

        #region Properties

        public List<IMission> MachineMissions { get; set; }

        #endregion
    }
}
