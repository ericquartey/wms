using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.NordDriver
{
    internal partial class NordDriverService : AutomationBackgroundService<FieldCommandMessage, FieldNotificationMessage, FieldCommandEvent, FieldNotificationEvent>
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private bool isDisposed;

        #endregion

        #region Constructors

        public NordDriverService(
            ILogger<NordDriverService> logger,
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Methods

        public void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.isDisposed = true;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            await base.StartAsync(cancellationToken);
        }

        protected override bool FilterCommand(FieldCommandMessage command)
        {
            return
                command.Destination == FieldMessageActor.InverterDriver
                ||
                command.Destination == FieldMessageActor.Any;
        }

        protected override bool FilterNotification(FieldNotificationMessage notification)
        {
            return
                notification.Destination == FieldMessageActor.InverterDriver
                ||
                notification.Destination == FieldMessageActor.Any;
        }

        protected override Task OnCommandReceivedAsync(FieldCommandMessage receivedMessage, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace($"1:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}");

            var inverterIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            // TODO implement commands

            return Task.CompletedTask;
        }

        protected override async Task OnNotificationReceivedAsync(FieldNotificationMessage receivedMessage, IServiceProvider serviceProvider)
        {
            var inverterIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());
            // TODO close state machine

            if (receivedMessage.Source == FieldMessageActor.InverterDriver
                && (receivedMessage.Status == MessageStatus.OperationEnd
                    || receivedMessage.Status == MessageStatus.OperationStop
                    )
                )
            {
                var notificationMessageToFsm = receivedMessage;

                // forward the message to upper level
                notificationMessageToFsm.Destination = FieldMessageActor.DeviceManager;

                this.eventAggregator?
                    .GetEvent<FieldNotificationEvent>()
                    .Publish(notificationMessageToFsm);
            }
        }

        #endregion
    }
}
