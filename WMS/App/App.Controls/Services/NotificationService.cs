using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Resources;
using Ferretto.WMS.Data.WebAPI.Contracts;
using NLog;

namespace Ferretto.WMS.App.Controls.Services
{
    public class NotificationService : INotificationService
    {
        #region Fields

        private readonly IDataHubClient dataHubClient;

        private readonly IDialogService dialogService;

        private readonly IEventService eventService;

        private readonly Logger logger;

        private bool isServiceHubConnected;

        #endregion

        #region Constructors

        public NotificationService(
            IEventService eventService,
            IDialogService dialogService,
            IDataHubClient dataHubClient)
        {
            this.eventService = eventService;
            this.dialogService = dialogService;
            this.dataHubClient = dataHubClient;
            this.dataHubClient.EntityChanged += this.DataHubClient_EntityChanged;
            this.dataHubClient.MachineStatusUpdated += this.DataHubClient_MachineStatusUpdated;
            this.dataHubClient.ConnectionStatusChanged += this.DataHubClient_ConnectionStatusChanged;

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
            await this.dataHubClient.DisconnectAsync();
        }

        public async Task StartAsync()
        {
            await this.dataHubClient.ConnectAsync();
        }

        private void DataHubClient_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            this.IsServiceHubConnected = e.IsConnected;
        }

        private void DataHubClient_EntityChanged(object sender, EntityChangedEventArgs e)
        {
            this.logger.Debug($"EntityChangedEvent received from server: Operation={e.Operation}, " +
                $"EntityType={e.EntityType}, Id={e.Id}, " +
                $"SourceEntityType={e.SourceEntityType}, SourceId={e.SourceId}");

            this.eventService
                .Invoke(new ModelChangedPubSubEvent(
                    e.EntityType,
                    e.Id,
                    e.Operation,
                    e.SourceEntityType,
                    e.SourceId));
        }

        private void DataHubClient_MachineStatusUpdated(object sender, MachineStatusUpdatedEventArgs e)
        {
            this.eventService
                .Invoke(new MachineStatusPubSubEvent(e.MachineStatus));
        }

        private void NotifyErrorDialog()
        {
            var msg = this.isServiceHubConnected ? General.ConnetionToDataServiceRestored : General.ErrorOnConnetionToDataService;
            this.dialogService.ShowErrorDialog(General.ConnectionStatus, msg, this.isServiceHubConnected == false);
        }

        #endregion
    }
}
