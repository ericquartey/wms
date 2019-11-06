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

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
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

        protected override bool FilterCommand(CommandMessage command)
        {
            return true;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return true;
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
                ErrorLevel.Critical);

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command, IServiceProvider serviceProvider)
        {
            serviceProvider
                .GetRequiredService<ILogEntriesProvider>()
                .Add(command);

            return Task.CompletedTask;
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage notification, IServiceProvider serviceProvider)
        {
            serviceProvider
                .GetRequiredService<ILogEntriesProvider>()
                .Add(notification);

            return Task.CompletedTask;
        }

        private void SendErrorMessage(IMessageData data)
        {
            var message = new NotificationMessage(
                data,
                "DataLayer Error",
                MessageActor.Any,
                MessageActor.DataLayer,
                MessageType.DlException,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(message);
        }

        #endregion
    }
}
