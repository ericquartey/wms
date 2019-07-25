using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.LowSpeedMovements
{
    public class LSMTCarouselViewModel : BindableBase, ILSMTCarouselViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private DelegateCommand closeButtonCommand;

        private IUnityContainer container;

        private string currentPosition;

        private DelegateCommand openButtonCommand;

        private IPositioningService positioningService;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateCurrentPositionToken;

        #endregion

        #region Constructors

        public LSMTCarouselViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public DelegateCommand CloseButtonCommand => this.closeButtonCommand ?? (this.closeButtonCommand = new DelegateCommand(async () => await this.CloseCarouselAsync()));

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DelegateCommand OpenButtonCommand => this.openButtonCommand ?? (this.openButtonCommand = new DelegateCommand(async () => await this.OpenCarouselAsync()));

        public DelegateCommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopCarouselAsync()));

        #endregion

        #region Methods

        public async Task CloseCarouselAsync()
        {
            var messageData = new MovementMessageDataDto { Axis = Axis.Both, MovementType = MovementType.Absolute, SpeedPercentage = 50, Displacement = -100m };
            await this.positioningService.ExecuteAsync(messageData);
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.positioningService = this.container.Resolve<IPositioningService>();
        }

        public Task OnEnterViewAsync()
        {
            this.updateCurrentPositionToken = this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data.CurrentPosition),
                ThreadOption.PublisherThread,
                false);

            return Task.CompletedTask;
        }

        public async Task OpenCarouselAsync()
        {
            var messageData = new MovementMessageDataDto { Axis = Axis.Both, MovementType = MovementType.Absolute, SpeedPercentage = 50, Displacement = 100m };
            await this.positioningService.ExecuteAsync(messageData);
        }

        public async Task StopCarouselAsync()
        {
            await this.positioningService.StopAsync();
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
