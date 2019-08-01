using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.LowSpeedMovements
{
    public class LSMTVerticalEngineViewModel : BindableBase, ILSMTVerticalEngineViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IPositioningMachineService positioningService;

        private string currentPosition;

        private bool isButtonDownEnabled;

        private bool isButtonUpEnabled;

        private DelegateCommand moveDownButtonCommand;

        private DelegateCommand moveUpButtonCommand;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateCurrentPositionToken;

        #endregion

        #region Constructors

        public LSMTVerticalEngineViewModel(
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

        public async Task MoveDownVerticalAxisAsync()
        {
            this.IsButtonUpEnabled = false;
            var messageData = new MovementMessageDataDto { Axis = Axis.Vertical, MovementType = MovementType.Relative, SpeedPercentage = 0, Displacement = -1.0m };
            await this.positioningService.ExecuteAsync(messageData);
        }

        public async Task MoveUpVerticalAxisAsync()
        {
            this.IsButtonDownEnabled = false;
            var messageData = new MovementMessageDataDto { Axis = Axis.Vertical, MovementType = MovementType.Relative, SpeedPercentage = 0, Displacement = 1.0m };
            await this.positioningService.ExecuteAsync(messageData);
        }

        public Task OnEnterViewAsync()
        {
            this.updateCurrentPositionToken = this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data.CurrentPosition),
                ThreadOption.PublisherThread,
                false);

            this.IsButtonUpEnabled = true;
            this.IsButtonDownEnabled = true;

            return Task.CompletedTask;
        }

        public async Task StopVerticalAxisAsync()
        {
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
