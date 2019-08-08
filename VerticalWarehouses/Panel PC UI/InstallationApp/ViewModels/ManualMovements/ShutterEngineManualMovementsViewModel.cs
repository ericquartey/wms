using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ShutterEngineManualMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly DelegateCommand currentShutterPosition;

        private readonly IShutterMachineService shutterService;

        private readonly ITestMachineService testService;

        private ShutterPosition? currentPosition;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        private DelegateCommand stopMovementCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public ShutterEngineManualMovementsViewModel(IShutterMachineService shutterService)
            : base(Services.PresentationMode.Installator)
        {
            if (shutterService == null)
            {
                throw new System.ArgumentNullException(nameof(shutterService));
            }

            this.shutterService = shutterService;
        }

        #endregion

        #region Properties

        public ShutterPosition? CurrentPosition
        {
            get => this.currentPosition;
            set => this.SetProperty(ref this.currentPosition, value);
        }

        public DelegateCommand MoveDownCommand =>
            this.moveDownCommand
            ??
            (this.moveDownCommand = new DelegateCommand(async () => await this.MoveDownAsync()));

        public DelegateCommand MoveUpCommand =>
            this.moveUpCommand
            ??
            (this.moveUpCommand = new DelegateCommand(async () => await this.MoveUpAsync()));

        public BindableBase NavigationViewModel { get; set; }

        public DelegateCommand StopMovementCommand =>
            this.stopMovementCommand
            ??
            (this.stopMovementCommand = new DelegateCommand(async () => await this.StopMovementAsync()));

        #endregion

        #region Methods

        public async Task MoveDownAsync()
        {
            var messageData = new ShutterPositioningMovementMessageDataDto
            {
                BayNumber = 1,
                ShutterPositionMovement = MAS.AutomationService.Contracts.ShutterMovementDirection.Down
            };

            await this.ExecutePositioningAsync(messageData);
        }

        public async Task MoveUpAsync()
        {
            var messageData = new ShutterPositioningMovementMessageDataDto
            {
                BayNumber = 1,
                ShutterPositionMovement = MAS.AutomationService.Contracts.ShutterMovementDirection.Up
            };

            await this.ExecutePositioningAsync(messageData);
        }

        public override void OnNavigated()
        {
            this.subscriptionToken = this.EventAggregator
              .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
              .Subscribe(
                  message => this.CurrentPosition = message?.Data?.ShutterPosition,
                  ThreadOption.PublisherThread,
                  false);

            base.OnNavigated();
        }

        public async Task StopMovementAsync()
        {
            try
            {
                await this.shutterService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowError(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.subscriptionToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }

            base.Dispose(disposing);
        }

        private async Task ExecutePositioningAsync(ShutterPositioningMovementMessageDataDto messageData)
        {
            try
            {
                await this.shutterService.ExecutePositioningAsync(messageData);
            }
            catch (System.Exception ex)
            {
                this.ShowError(ex);
            }
        }

        #endregion
    }
}
