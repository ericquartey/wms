using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal partial class DataLayerService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>, IDataLayerService
    {
        #region Constructors

        public DataLayerService(
            IEventAggregator eventAggregator,
            ILogger<DataLayerService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
        }

        #endregion

        #region Properties

        public bool IsReady { get; private set; }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            await this.InitializeAsync();
        }

        protected override void NotifyCommandError(CommandMessage notificationData)
        {
            this.Logger.LogDebug($"Notifying Data Layer service error");

            var msg = new NotificationMessage(
                notificationData.Data,
                "DL Error",
                MessageActor.Any,
                MessageActor.MachineManager,
                MessageType.FsmException,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(msg);
        }

        private void SendErrorMessage(IMessageData data)
        {
            var errorLevel = data?.Verbosity is MessageVerbosity.Fatal
                ? ErrorLevel.Fatal
                : ErrorLevel.Error;

            var message = new NotificationMessage(
                data,
                "DataLayer Error",
                MessageActor.Any,
                MessageActor.DataLayer,
                MessageType.DlException,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationError,
                errorLevel);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(message);
        }

        #endregion
    }
}
