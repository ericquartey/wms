using System.Threading.Tasks;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.MAS_Utils.Enumerations;
// TEMP To be removed
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTShutterEngineViewModel : BindableBase, ILSMTShutterEngineViewModel
    {
        #region Fields

        private readonly DelegateCommand currentShutterPosition;

        private readonly IEventAggregator eventAggregator;

        private DelegateCommand closeButtonCommand;

        private IUnityContainer container;

        private string currentPosition;

        private IInstallationService installationService;

        private DelegateCommand openButtonCommand;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateShutterPositioningToken;

        #endregion

        #region Constructors

        public LSMTShutterEngineViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.CurrentPosition = ShutterPosition.Closed.ToString();
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
            var messageData = new ShutterPositioningMovementMessageDataDTO { BayNumber = 1, ShutterPositionMovement = 0 };
            await this.installationService.ExecuteShutterPositioningMovementAsync(messageData);
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
            this.updateShutterPositioningToken = this.eventAggregator.GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data.ShutterPosition),
                ThreadOption.PublisherThread,
                false);
        }

        public async Task OpenShutterAsync()
        {
            var messageData = new ShutterPositioningMovementMessageDataDTO { BayNumber = 1, ShutterPositionMovement = 1 };
            await this.installationService.ExecuteShutterPositioningMovementAsync(messageData);
        }

        public async Task StopShutterAsync()
        {
            await this.installationService.StopCommandAsync();
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<ShutterPositioningMessageData>>().Unsubscribe(this.updateShutterPositioningToken);
        }

        public void UpdateCurrentPosition(ShutterPosition shutterPosition)
        {
            this.CurrentPosition = shutterPosition.ToString();
        }

        #endregion
    }
}
