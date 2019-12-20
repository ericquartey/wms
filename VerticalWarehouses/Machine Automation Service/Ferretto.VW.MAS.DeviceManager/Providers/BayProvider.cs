using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal sealed class BayProvider : BaseProvider, IBayProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ILogger<BayProvider> logger;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public BayProvider(
            IBaysDataProvider baysDataProvider,
            ISensorsProvider sensorsProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IEventAggregator eventAggregator,
            ILogger<BayProvider> logger)
            : base(eventAggregator)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void LightOn(bool enable, BayNumber bayNumber, MessageActor sender)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
