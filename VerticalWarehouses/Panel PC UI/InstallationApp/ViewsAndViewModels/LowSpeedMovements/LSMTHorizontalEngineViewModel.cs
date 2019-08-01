using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.LowSpeedMovements
{
    public class LSMTHorizontalEngineViewModel : BindableBase, ILSMTHorizontalEngineViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IPositioningMachineService positioningService;

        private string currentPosition;

        private bool isButtonBackEnabled;

        private bool isButtonForwardEnabled;

        private DelegateCommand moveBackwardButtonCommand;

        private DelegateCommand moveForwardButtonCommand;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateCurrentPositionToken;

        #endregion

        #region Constructors

        public LSMTHorizontalEngineViewModel(
            IEventAggregator eventAggregator,
            IPositioningMachineService positioningService)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (positioningService == null)
            {
                throw new System.ArgumentNullException(nameof(positioningService));
            }

            this.eventAggregator = eventAggregator;
            this.positioningService = positioningService;
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

        public Task OnEnterViewAsync()
        {
            this.IsButtonBackEnabled = true;
            this.IsButtonForwardEnabled = true;

            this.updateCurrentPositionToken = this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data.CurrentPosition),
                ThreadOption.PublisherThread,
                false);

            return Task.CompletedTask;
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

            var messageData = new MovementMessageDataDto { Axis = Axis.Horizontal, MovementType = MovementType.Relative, SpeedPercentage = 0, Displacement = -1.0m };
            await this.positioningService.ExecuteAsync(messageData);
        }

        private async Task MoveForwardHorizontalAxisHandlerAsync()
        {
            this.IsButtonBackEnabled = false;
            var messageData = new MovementMessageDataDto { Axis = Axis.Horizontal, MovementType = MovementType.Relative, SpeedPercentage = 0, Displacement = 1.0m };
            await this.positioningService.ExecuteAsync(messageData);
        }

        private async Task StopHorizontalAxisHandlerAsync()
        {
            this.IsButtonBackEnabled = true;
            this.IsButtonForwardEnabled = true;
            await this.positioningService.StopAsync();
        }

        #endregion
    }
}
