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

namespace Ferretto.VW.MAS.LaserDriver
{
    public sealed class LaserDriverService : AutomationBackgroundService<FieldCommandMessage, FieldNotificationMessage, FieldCommandEvent, FieldNotificationEvent>
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IConfiguration configuration;

        private readonly IDigitalDevicesDataProvider digitalDevicesDataProvider;

        private IDictionary<BayNumber, LaserDevice> lasers;

        #endregion

        #region Constructors

        public LaserDriverService(
            IEventAggregator eventAggregator,
            IDigitalDevicesDataProvider digitalDevicesDataProvider,
            IBaysDataProvider baysDataProvider,
            ILogger<LaserDriverService> logger,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.digitalDevicesDataProvider = digitalDevicesDataProvider ?? throw new ArgumentNullException(nameof(digitalDevicesDataProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #endregion

        #region Methods

        protected override bool FilterCommand(FieldCommandMessage command)
        {
            return
                command.Destination == FieldMessageActor.LaserDriver
                ||
                command.Destination == FieldMessageActor.Any;
        }

        protected override bool FilterNotification(FieldNotificationMessage notification)
        {
            return
                notification.Destination == FieldMessageActor.LaserDriver
                ||
                notification.Destination == FieldMessageActor.Any;
        }

        protected override async Task OnCommandReceivedAsync(FieldCommandMessage command, IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        protected override async Task OnNotificationReceivedAsync(FieldNotificationMessage message, IServiceProvider serviceProvider)
        {
            switch (message.Type)
            {
                case FieldMessageType.DataLayerReady:

                    this.InitializeLaserDevices();
                    //await this.StartHardwareCommunicationsAsync(serviceProvider);
                    break;
            }
        }

        private void InitializeLaserDevices()
        {
            var lasersDto = this.digitalDevicesDataProvider.GetAllLasers();
            this.lasers = lasersDto.ToDictionary(x => x.Bay.Number, y => new LaserDevice(y.IpAddress, y.TcpPort));
        }

        #endregion
    }
}
