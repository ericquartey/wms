using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using NLog;

namespace Ferretto.WMS.App.Controls.Services
{
    public class NotificationService : INotificationService
    {
        #region Fields

        private readonly IDialogService dialogService;

        private readonly IEventService eventService;

        private readonly Logger logger;

        private readonly ISchedulerHubClient schedulerHubClient;

        private bool isServiceHubConnected;

        #endregion

        #region Constructors

        public NotificationService(
            IEventService eventService,
            IDialogService dialogService,
            ISchedulerHubClient schedulerHubClient)
        {
            this.eventService = eventService;
            this.dialogService = dialogService;
            this.schedulerHubClient = schedulerHubClient;
            this.schedulerHubClient.EntityChanged += this.SchedulerHubClient_EntityChanged;
            this.schedulerHubClient.ConnectionStatusChanged += this.SchedulerHubClient_ConnectionStatusChanged;

            this.logger = LogManager.GetCurrentClassLogger();
        }

        #endregion

        #region Properties

        public bool IsServiceHubConnected
        {
            get => this.isServiceHubConnected;
            private set
            {
                if (value != this.isServiceHubConnected)
                {
                    this.isServiceHubConnected = value;
                    this.NotifyErrorDialog();
                    this.eventService.Invoke(new StatusPubSubEvent { IsSchedulerOnline = this.isServiceHubConnected });
                }
            }
        }

        #endregion

        #region Methods

        public void CheckForDataErrorConnection()
        {
            if (this.isServiceHubConnected == false)
            {
                this.NotifyErrorDialog();
            }
        }

        public async Task EndAsync()
        {
            await this.schedulerHubClient.DisconnectAsync();
        }

        public async Task StartAsync()
        {
            await this.schedulerHubClient.ConnectAsync();
        }

        private void NotifyErrorDialog()
        {
            var msg = this.isServiceHubConnected ? General.ConnetionToDataServiceRestored : General.ErrorOnConnetionToDataService;
            this.dialogService.ShowErrorDialog(General.ConnectionStatus, msg, this.isServiceHubConnected == false);
        }

        private void SchedulerHubClient_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            this.IsServiceHubConnected = e.IsConnected;
        }

        private void SchedulerHubClient_EntityChanged(object sender, EntityChangedEventArgs e)
        {
            this.logger.Debug($"Message {e.EntityType}, operation {e.Operation} received from server");

            this.eventService
                .Invoke(new ModelChangedPubSubEvent(e.EntityType, e.Id, e.Operation));
        }

        #endregion
    }
}
