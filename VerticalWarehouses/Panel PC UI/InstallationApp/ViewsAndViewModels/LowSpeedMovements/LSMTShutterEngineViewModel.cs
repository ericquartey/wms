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
    public class LSMTShutterEngineViewModel : BindableBase, ILSMTShutterEngineViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private DelegateCommand closeButtonCommand;

        private IUnityContainer container;

        private string currentPosition;

        private IInstallationService installationService;

        private DelegateCommand openButtonCommand;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateCurrentPositionToken;

        #endregion

        #region Constructors

        public LSMTShutterEngineViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public DelegateCommand CloseButtonCommand => this.closeButtonCommand ?? (this.closeButtonCommand = new DelegateCommand(async () => await this.CloseShutterAsync()));

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public DelegateCommand OpenButtonCommand => this.openButtonCommand ?? (this.openButtonCommand = new DelegateCommand(async () => await this.OpenShutterAsync()));

        public DelegateCommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopShutterAsync()));

        #endregion

        #region Methods

        public async Task CloseShutterAsync()
        {
            var messageData = new MovementMessageDataDTO { Axis = 2, MovementType = 1, SpeedPercentage = 50, Displacement = -100m };
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
            var messageData = new MovementMessageDataDTO { Axis = 1, MovementType = 1, SpeedPercentage = 50, Displacement = 100m };
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

        #endregion
    }
}
