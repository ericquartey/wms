using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Microsoft.Extensions.Hosting;

namespace Ferretto.VW.MAS.LaserDriver.HostedServices
{
    internal sealed class LaserDriverService : AutomationBackgroundService<FieldCommandMessage, FieldNotificationMessage, FieldCommandEvent, FieldNotificationEvent>
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IConfiguration configuration;

        #endregion

        #region Constructors

        public LaserDriverService(
            IEventAggregator eventAggregator,
            IBaysDataProvider baysDataProvider,
            ILogger<LaserDriverService> logger,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #endregion

        #region Methods

        protected override bool FilterCommand(FieldCommandMessage command)
        {
            throw new NotImplementedException();
        }

        protected override bool FilterNotification(FieldNotificationMessage notification)
        {
            throw new NotImplementedException();
        }

        protected override Task OnCommandReceivedAsync(FieldCommandMessage command, IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        protected override Task OnNotificationReceivedAsync(FieldNotificationMessage message, IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
