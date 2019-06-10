using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.MAS_Utils.Events;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTHorizontalEngineViewModel : BindableBase, ILSMTHorizontalEngineViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private string currentPosition;

        private IInstallationService installationService;

        private bool isButtonBackEnabled;

        private bool isButtonForwardEnabled;

        private DelegateCommand moveBackwardButtonCommand;

        private DelegateCommand moveForwardButtonCommand;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateCurrentPositionToken;

        #endregion

        #region Constructors

        public LSMTHorizontalEngineViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public bool IsButtonBackEnabled { get => this.isButtonBackEnabled; set => this.SetProperty(ref this.isButtonBackEnabled, value); }

        public bool IsButtonForwardEnabled { get => this.isButtonForwardEnabled; set => this.SetProperty(ref this.isButtonForwardEnabled, value); }

        public DelegateCommand MoveBackwardButtonCommand => this.moveBackwardButtonCommand ??
                                    (this.moveBackwardButtonCommand = new DelegateCommand(async () => await this.MoveBackHorizontalAxisHandlerAsync()));

        public DelegateCommand MoveForwardButtonCommand => this.moveForwardButtonCommand ??
            (this.moveForwardButtonCommand = new DelegateCommand(async () => await this.MoveForwardHorizontalAxisHandlerAsync()));

        public BindableBase NavigationViewModel { get; set; }

        public DelegateCommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopHorizontalAxisHandlerAsync()));

        #endregion

        #region Methods

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
            this.IsButtonBackEnabled = true;
            this.IsButtonForwardEnabled = true;

            this.updateCurrentPositionToken = this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data.CurrentPosition),
                ThreadOption.PublisherThread,
                false);
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.updateCurrentPositionToken);
        }

        public void UpdateCurrentPosition(decimal currentPosition)
        {
            this.CurrentPosition = currentPosition.ToString();
        }

        private async Task MoveBackHorizontalAxisHandlerAsync()
        {
            this.IsButtonForwardEnabled = false;

            var messageData = new MovementMessageDataDTO { Axis = Axis.Horizontal, MovementType = MovementType.Relative, SpeedPercentage = 0, Displacement = -1.0m };
            await this.installationService.ExecuteMovementAsync(messageData);
        }

        private async Task MoveForwardHorizontalAxisHandlerAsync()
        {
            this.IsButtonBackEnabled = false;

            var messageData = new MovementMessageDataDTO { Axis = Axis.Horizontal, MovementType = MovementType.Relative, SpeedPercentage = 0, Displacement = 1.0m };
            await this.installationService.ExecuteMovementAsync(messageData);
        }

        private async Task StopHorizontalAxisHandlerAsync()
        {
            this.IsButtonBackEnabled = true;
            this.IsButtonForwardEnabled = true;
            await this.installationService.StopCommandAsync();
        }

        #endregion
    }
}
