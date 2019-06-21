using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.MAS_Utils.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTVerticalEngineViewModel : BindableBase, ILSMTVerticalEngineViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private string currentPosition;

        //private IInstallationService installationService;

        private bool isButtonDownEnabled;

        private bool isButtonUpEnabled;

        private DelegateCommand moveDownButtonCommand;

        private DelegateCommand moveUpButtonCommand;

        private IPositioningService positioningService;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateCurrentPositionToken;

        #endregion

        #region Constructors

        public LSMTVerticalEngineViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public bool IsButtonDownEnabled { get => this.isButtonDownEnabled; set => this.SetProperty(ref this.isButtonDownEnabled, value); }

        public bool IsButtonUpEnabled { get => this.isButtonUpEnabled; set => this.SetProperty(ref this.isButtonUpEnabled, value); }

        public DelegateCommand MoveDownButtonCommand => this.moveDownButtonCommand ?? (this.moveDownButtonCommand = new DelegateCommand(async () => await this.MoveDownVerticalAxisAsync()));

        public DelegateCommand MoveUpButtonCommand => this.moveUpButtonCommand ?? (this.moveUpButtonCommand = new DelegateCommand(async () => await this.MoveUpVerticalAxisAsync()));

        public BindableBase NavigationViewModel { get; set; }

        public DelegateCommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopVerticalAxisAsync()));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            //this.installationService = this.container.Resolve<IInstallationService>();

            this.positioningService = this.container.Resolve<IPositioningService>();
        }

        public async Task MoveDownVerticalAxisAsync()
        {
            this.IsButtonUpEnabled = false;
            var messageData = new MovementMessageDataDTO { Axis = Axis.Vertical, MovementType = MovementType.Relative, SpeedPercentage = 0, Displacement = -1.0m };
            //await this.installationService.ExecuteMovementAsync(messageData);
            await this.positioningService.ExecuteAsync(messageData);
        }

        public async Task MoveUpVerticalAxisAsync()
        {
            this.IsButtonDownEnabled = false;
            var messageData = new MovementMessageDataDTO { Axis = Axis.Vertical, MovementType = MovementType.Relative, SpeedPercentage = 0, Displacement = 1.0m };
            //await this.installationService.ExecuteMovementAsync(messageData);
            await this.positioningService.ExecuteAsync(messageData);
        }

        public async Task OnEnterViewAsync()
        {
            this.updateCurrentPositionToken = this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data.CurrentPosition),
                ThreadOption.PublisherThread,
                false);

            this.IsButtonUpEnabled = true;
            this.IsButtonDownEnabled = true;
        }

        public async Task StopVerticalAxisAsync()
        {
            //await this.installationService.StopCommandAsync();
            await this.positioningService.StopAsync();
            this.IsButtonDownEnabled = true;
            this.IsButtonUpEnabled = true;
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.updateCurrentPositionToken);
        }

        public void UpdateCurrentPosition(decimal currentPosition)
        {
            this.CurrentPosition = currentPosition.ToString();
        }

        #endregion
    }
}
