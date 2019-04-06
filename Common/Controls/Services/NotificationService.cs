using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Resources;
using Ferretto.WMS.Data.WebAPI.Contracts;
using NLog;

namespace Ferretto.Common.Controls.Services
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

        public static IPubSubEvent GetInstanceOfModelChanged(EntityChangedEventArgs e)
        {
            if (e == null)
            {
                return null;
            }

            var modelsAssembly = ConfigurationManager.AppSettings["ModelsAssembly"];
            var modelsNamespace = ConfigurationManager.AppSettings["ModelsNamespace"];
            var entityName = $"{modelsNamespace}.{e.EntityType},{modelsAssembly}";
            var entity = Type.GetType(entityName);
            if (entity == null)
            {
                throw new InvalidOperationException(string.Format(Errors.UnableToResolveEntity, entityName));
            }

            var constructedClass = typeof(ModelChangedPubSubEvent<,>).MakeGenericType(entity, typeof(int));
            return Activator.CreateInstance(constructedClass, e.Id) as IPubSubEvent;
        }

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

            var modelInstance = GetInstanceOfModelChanged(e);
            if (modelInstance != null)
            {
                this.eventService.DynamicInvoke(modelInstance);
            }
        }

        #endregion
    }
}
