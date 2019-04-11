using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.InstallationApp.Interfaces;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.MAS_AutomationService.Contracts;
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
        }

        #endregion

        #region Properties

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public DelegateCommand MoveDownButtonCommand => this.moveDownButtonCommand ?? (this.moveDownButtonCommand = new DelegateCommand(async () => await this.MoveDownVerticalAxisAsync()));

        public DelegateCommand MoveUpButtonCommand => this.moveUpButtonCommand ?? (this.moveUpButtonCommand = new DelegateCommand(async () => await this.MoveUpVerticalAxisAsync()));

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
            var messageData = new MovementMessageDataDTO { Axis = 0, MovementType = 1, SpeedPercentage = 50, Displacement = -100m };
            await this.installationService.ExecuteMovementAsync(messageData);
        }

        public async Task MoveUpVerticalAxisAsync()
        {
            var messageData = new MovementMessageDataDTO { Axis = 1, MovementType = 1, SpeedPercentage = 50, Displacement = 100m };
            await this.installationService.ExecuteMovementAsync(messageData);
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

        public async Task StopVerticalAxisAsync()
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

        #endregion
    }
}
