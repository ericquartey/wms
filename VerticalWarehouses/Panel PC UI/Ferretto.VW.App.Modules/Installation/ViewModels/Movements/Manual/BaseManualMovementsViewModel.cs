using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal abstract class BaseManualMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private Bay bay;

        private int bayNumber;

        private bool isStopping;

        private bool isWaitingForResponse;

        private SubscriptionToken movementsSubscriptionToken;

        private DelegateCommand stopMovementCommand;

        #endregion

        #region Constructors

        protected BaseManualMovementsViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            this.MachineElevatorService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
        }

        #endregion

        #region Properties

        public int BayNumber
        {
            get => this.bayNumber;
            protected set => this.SetProperty(ref this.bayNumber, value);
        }

        public bool IsStopping
        {
            get => this.isStopping;
            protected set
            {
                if (this.SetProperty(ref this.isStopping, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            private set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand StopMovementCommand =>
            this.stopMovementCommand
            ??
            (this.stopMovementCommand = new DelegateCommand(async () => await this.StopMovementAsync(), this.CanExecuteStopMovment));

        protected IMachineElevatorWebService MachineElevatorService { get; }

        #endregion

        #region Methods

        public void DisableAllExceptThis()
        {
            var name = this.GetType().ToString();
            this.EventAggregator
                .GetEvent<ManualMovementsChangedPubSubEvent>()
                .Publish(new ManualMovementsChangedMessage(name));
        }

        public override void Disappear()
        {
            base.Disappear();

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.notificationUIsubscriptionToken?.Dispose();
            this.notificationUIsubscriptionToken = null;

            this.movementsSubscriptionToken?.Dispose();
            this.movementsSubscriptionToken = null;
            */
        }

        public void EnableAll()
        {
            this.EventAggregator
               .GetEvent<ManualMovementsChangedPubSubEvent>()
               .Publish(new ManualMovementsChangedMessage(null));
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.SubscribeToEvents();

            await this.RetrieveCurrentPositionAsync();

            await base.OnAppearedAsync();

            this.EnableAll();
        }

        protected virtual bool CanExecuteStopMovment()
        {
            return true;
        }

        protected virtual void EnabledChanged(ManualMovementsChangedMessage message)
        {
            if (string.IsNullOrEmpty(message.ViewModelName))
            {
                this.IsEnabled = true;
                return;
            }

            var name = this.GetType().ToString();
            if (!name.Equals(message.ViewModelName))
            {
                this.IsEnabled = false;
            }
        }

        protected abstract void OnMachinePowerChanged();

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState != MachinePowerState.Powered)
            {
            }

            this.OnMachinePowerChanged();
        }

        protected virtual void RaiseCanExecuteChanged()
        {
        }

        protected abstract Task StopMovementAsync();

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.bay = await this.bayManagerService.GetBayAsync();
                this.BayNumber = (int)this.bay.Number;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void SubscribeToEvents()
        {
            this.movementsSubscriptionToken = this.movementsSubscriptionToken
                ??
                this.EventAggregator
                    .GetEvent<ManualMovementsChangedPubSubEvent>()
                    .Subscribe(
                        this.EnabledChanged,
                        ThreadOption.UIThread,
                        false,
                        message => message != null);
        }

        #endregion
    }
}
