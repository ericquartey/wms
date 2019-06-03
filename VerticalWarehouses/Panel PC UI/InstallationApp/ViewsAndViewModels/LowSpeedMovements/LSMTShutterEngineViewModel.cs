using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_AutomationService.Contracts;
// TEMP To be removed
using Ferretto.VW.MAS_Utils.Events;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ShutterMovementDirection = Ferretto.VW.MAS_AutomationService.Contracts.ShutterMovementDirection;

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

        private ITestService testService;

        private SubscriptionToken updateShutterPositioningToken;

        #endregion

        #region Constructors

        public LSMTShutterEngineViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
            this.CurrentPosition = ShutterPosition.Closed.ToString();
        }

        #endregion

        #region Properties

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public DelegateCommand DownButtonCommand => this.closeButtonCommand ?? (this.closeButtonCommand = new DelegateCommand(async () => await this.DownShutterAsync()));

        public BindableBase NavigationViewModel { get; set; }

        public DelegateCommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopShutterAsync()));

        public DelegateCommand UpButtonCommand => this.openButtonCommand ?? (this.openButtonCommand = new DelegateCommand(async () => await this.UpShutterAsync()));

        #endregion

        #region Methods

        public async Task DownShutterAsync()
        {
            var messageData = new ShutterPositioningMovementMessageDataDTO { BayNumber = 1, ShutterPositionMovement = 0 };
            //await this.installationService.ExecuteShutterPositioningMovementAsync(messageData);
            //TEMP
            //await this.testService.ExecuteShutterPositioningMovementTestAsync(messageData);
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.installationService = this.container.Resolve<IInstallationService>();
            // TEMP
            //this.testService = this.container.Resolve<ITestService>();
        }

        public async Task OnEnterViewAsync()
        {
            this.updateShutterPositioningToken = this.eventAggregator.GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data.ShutterPosition),
                ThreadOption.PublisherThread,
                false);
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

        public async Task UpShutterAsync()
        {
            var messageData = new ShutterPositioningMovementMessageDataDTO { BayNumber = 1, ShutterPositionMovement = ShutterMovementDirection.Up };
            //await this.installationService.ExecuteShutterPositioningMovementAsync(messageData);
            //TEMP
            //await this.testService.ExecuteShutterPositioningMovementTestAsync(messageData);
        }

        #endregion
    }
}
