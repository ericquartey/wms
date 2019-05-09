using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.MAS_Utils.Events;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTVerticalEngineViewModel : BindableBase, ILSMTVerticalEngineViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private string currentPosition;

        private IInstallationService installationService;

        private DelegateCommand moveDownButtonCommand;

        private DelegateCommand moveUpButtonCommand;

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
            this.installationService = this.container.Resolve<IInstallationService>();
        }

        public async Task MoveDownVerticalAxisAsync()
        {
            var messageData = new MovementMessageDataDTO { Axis = Axis.Vertical, MovementType = MovementType.Absolute, SpeedPercentage = 50, Displacement = -100m };
            await this.installationService.ExecuteMovementAsync(messageData);
        }

        public async Task MoveUpVerticalAxisAsync()
        {
            var messageData = new MovementMessageDataDTO { Axis = Axis.Horizontal, MovementType = MovementType.Absolute, SpeedPercentage = 50, Displacement = 100m };
            await this.installationService.ExecuteMovementAsync(messageData);
        }

        public async Task OnEnterViewAsync()
        {
            this.updateCurrentPositionToken = this.eventAggregator.GetEvent<NotificationEventUI<VerticalPositioningMessageData>>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data.CurrentPosition),
                ThreadOption.PublisherThread,
                false);
        }

        public async Task StopVerticalAxisAsync()
        {
            await this.installationService.StopCommandAsync();
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
