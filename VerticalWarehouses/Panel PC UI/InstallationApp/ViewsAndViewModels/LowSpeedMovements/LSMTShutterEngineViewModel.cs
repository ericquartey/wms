using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTShutterEngineViewModel : BindableBase, ILSMTShutterEngineViewModel
    {
        private readonly IEventAggregator eventAggregator;

        private DelegateCommand closeButtonCommand;

        private IUnityContainer container;

        private string currentPosition;

        private IInstallationService installationService;

        private DelegateCommand openButtonCommand;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateCurrentPositionToken;

        public LSMTShutterEngineViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public DelegateCommand CloseButtonCommand => this.closeButtonCommand ?? (this.closeButtonCommand = new DelegateCommand(async () => await this.CloseShutterAsync()));

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public DelegateCommand OpenButtonCommand => this.openButtonCommand ?? (this.openButtonCommand = new DelegateCommand(async () => await this.OpenShutterAsync()));

        public DelegateCommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopShutterAsync()));

        public async Task CloseShutterAsync()
        {
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Accept.Clear();
            //// INFO - 1st parameter the bay number
            //// INFO - 2nd parameter the movement: 1 = Up and 0 = Down
            //var messageData = new ShutterPositioningMovementMessageDataDTO(1, 0);
            //var json = JsonConvert.SerializeObject(messageData);
            //HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
            //await client.PostAsync(new Uri(string.Concat(this.installationUrl, this.executeMovementPath)), httpContent);

            //var messageData = new MovementMessageDataDTO { Axis = 2, MovementType = 1, SpeedPercentage = 50, Displacement = -100m };
            //await this.installationService.ExecuteMovementAsync(messageData);

            var messageData = new ShutterPositioningMovementMessageDataDTO(1, 0);
            await this.installationService.ExecuteMovementAsync(messageData);
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.installationService = this.container.Resolve<IInstallationService>();
        }

        public async Task OnEnterViewAsync()
        {
            this.updateCurrentPositionToken = this.eventAggregator.GetEvent<MAS_Event>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data),
                ThreadOption.PublisherThread,
                false,
                message => message.NotificationType == NotificationType.CurrentPosition || message.NotificationType == NotificationType.CurrentActionStatus);
        }

        public async Task OpenShutterAsync()
        {
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Accept.Clear();
            // INFO - 1st parameter the bay number
            // INFO - 2nd parameter the movement: 1 = Up and 0 = Down
            //var messageData = new ShutterPositioningMovementMessageDataDTO(1, 1);
            //var json = JsonConvert.SerializeObject(messageData);
            //HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
            //await client.PostAsync(new Uri(string.Concat(this.installationUrl, this.executeMovementPath)), httpContent);

            //var messageData = new MovementMessageDataDTO { Axis = 1, MovementType = 1, SpeedPercentage = 50, Displacement = 100m };
            //await this.installationService.ExecuteMovementAsync(messageData);

            var messageData = new ShutterPositioningMovementMessageDataDTO(1, 1);
            await this.installationService.ExecuteMovementAsync(messageData);
        }

        public async Task StopShutterAsync()
        {
            await this.installationService.StopCommandAsync();
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.updateCurrentPositionToken);
        }

        public void UpdateCurrentPosition(INotificationMessageData data)
        {
            if (data is INotificationActionUpdatedMessageData parsedData)
            {
                this.CurrentPosition = parsedData.CurrentEncoderPosition.ToString();
            }
        }
    }
}
