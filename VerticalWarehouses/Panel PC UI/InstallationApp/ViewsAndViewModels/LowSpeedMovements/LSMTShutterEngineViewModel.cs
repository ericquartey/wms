using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
// TEMP To be removed
using ShutterMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.ShutterMovementDirection;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.LowSpeedMovements
{
    public class LSMTShutterEngineViewModel : BindableBase, ILSMTShutterEngineViewModel
    {
        #region Fields

        private readonly DelegateCommand currentShutterPosition;

        private readonly IEventAggregator eventAggregator;

        private readonly IShutterMachineService shutterService;

        private readonly ITestMachineService testService;

        private DelegateCommand closeButtonCommand;

        private string currentPosition;

        private DelegateCommand openButtonCommand;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateShutterPositioningToken;

        #endregion

        #region Constructors

        public LSMTShutterEngineViewModel(
            IEventAggregator eventAggregator,
            IShutterMachineService shutterService)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (shutterService == null)
            {
                throw new System.ArgumentNullException(nameof(shutterService));
            }

            this.eventAggregator = eventAggregator;
            this.shutterService = shutterService;
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
            var messageData = new ShutterPositioningMovementMessageDataDto { BayNumber = 1, ShutterPositionMovement = ShutterMovementDirection.Down };
            await this.shutterService.ExecutePositioningAsync(messageData);
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public Task OnEnterViewAsync()
        {
            this.updateShutterPositioningToken = this.eventAggregator.GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data.ShutterPosition),
                ThreadOption.PublisherThread,
                false);

            return Task.CompletedTask;
        }

        public async Task StopShutterAsync()
        {
            await this.shutterService.StopAsync();
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
            var messageData = new ShutterPositioningMovementMessageDataDto { BayNumber = 1, ShutterPositionMovement = ShutterMovementDirection.Up };
            await this.shutterService.ExecutePositioningAsync(messageData);
        }

        #endregion
    }
}
