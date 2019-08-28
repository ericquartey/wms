using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Controls
{
    public abstract class BaseMainViewModel : BaseNavigationViewModel, IActivationViewModel
    {
        #region Fields

        private readonly IMachineModeService machineModeService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineModeService>();

        private bool isEnabled;

        private PresentationMode mode;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        protected BaseMainViewModel(PresentationMode mode)
        {
            this.mode = mode;
        }

        #endregion

        #region Properties

        public virtual EnableMask EnableMask => EnableMask.MachineMode | EnableMask.MachinePower;

        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

        public PresentationMode Mode
        {
            get => this.mode;
            set => this.SetProperty(ref this.mode, value);
        }

        #endregion

        #region Methods

        public override void Appear()
        {
            base.Appear();
        }

        public override void Disappear()
        {
            base.Disappear();

            if (this.subscriptionToken != null)
            {
                this.machineModeService.MachineModeChangedEvent
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }
        }

        public override async Task OnNavigatedAsync()
        {
            this.UpdatePresentation();

            this.subscriptionToken = this.machineModeService.MachineModeChangedEvent
               .Subscribe(
                   this.OnMachineModeChanged,
                   ThreadOption.UIThread,
                   false);

            this.UpdateIsEnabled(this.machineModeService.MachineMode, this.machineModeService.MachinePower);

            await base.OnNavigatedAsync();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            this.UpdatePresentation();
        }

        public void ShowNotification(string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Publish(new PresentationChangedMessage(message, severity));
        }

        public void ShowNotification(Exception exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Publish(new PresentationChangedMessage(exception));
        }

        protected virtual void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            this.UpdateIsEnabled(e.MachineMode, e.MachinePower);
        }

        private void UpdateIsEnabled(MachineMode machineMode, MachinePowerState machinePower)
        {
            if ((this.EnableMask & EnableMask.MachinePower) != EnableMask.None)
            {
                this.IsEnabled = machinePower != MachinePowerState.Unpowered;
            }
            else
            {
                this.IsEnabled = true;
            }
        }

        private void UpdatePresentation()
        {
            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Publish(new PresentationChangedMessage(this.Mode));
        }

        #endregion
    }
}
