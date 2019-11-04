using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Controls
{
    public abstract class BaseMainViewModel : BaseNavigationViewModel, IActivationViewModel
    {
        #region Fields

        private readonly IMachineModeService machineModeService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineModeService>();

        private bool isEnabled;

        private SubscriptionToken machineModeChangedToken;

        private SubscriptionToken machinePowerChangedToken;

        private PresentationMode mode;

        #endregion

        #region Constructors

        protected BaseMainViewModel(PresentationMode mode)
        {
            this.mode = mode;
        }

        #endregion

        #region Properties

        public virtual EnableMask EnableMask => EnableMask.MachinePoweredOn;

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

        public void ClearNotifications()
        {
            this.EventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(true));
        }

        public override void Disappear()
        {
            base.Disappear();

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.machineModeChangedToken?.Dispose();
            this.machineModeChangedToken = null;

            this.machinePowerChangedToken?.Dispose();
            this.machinePowerChangedToken = null;
            */
        }

        public override async Task OnAppearedAsync()
        {
            this.UpdatePresentation();

            this.machineModeChangedToken = this.machineModeChangedToken
                ??
                this.EventAggregator
                    .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
                    .Subscribe(
                       async e => await this.OnMachineModeChangedAsync(e),
                       ThreadOption.UIThread,
                       false);

            this.machinePowerChangedToken = this.machinePowerChangedToken
                ??
                this.EventAggregator
                    .GetEvent<PubSubEvent<MachinePowerChangedEventArgs>>()
                    .Subscribe(
                       async e => await this.OnMachinePowerChangedAsync(e),
                       ThreadOption.UIThread,
                       false);

            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                this.machineModeService.MachineMode);

            this.UpdateNotifications();

            await base.OnAppearedAsync();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            this.UpdatePresentation();
        }

        public void ShowAbortStep(bool isEnabled, bool isVisible)
        {
            this.ShowStep(PresentationTypes.Abort, isEnabled, isVisible);
        }

        public void ShowNextStep(bool isEnabled, bool isVisible, string moduleName = null, string viewName = null)
        {
            this.ShowStep(PresentationTypes.Next, isEnabled, isVisible, moduleName, viewName);
        }

        public void ShowNotification(string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            this.EventAggregator
                 .GetEvent<PresentationNotificationPubSubEvent>()
                 .Publish(new PresentationNotificationMessage(message, severity));
        }

        public void ShowNotification(Exception exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            this.EventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(exception));
        }

        public void ShowPrevStep(bool isVisible, bool isEnabled, string moduleName = null, string viewName = null)
        {
            this.ShowStep(PresentationTypes.Prev, isVisible, isEnabled, moduleName, viewName);
        }

        public void ShowStep(PresentationTypes presentationType, bool isVisible, bool isEnabled, string moduleName = null, string viewName = null)
        {
            var presentationStep = new PresentationStep()
            {
                Type = presentationType,
                IsEnabled = isEnabled,
                IsVisible = isVisible,
                ModuleName = moduleName,
                ViewName = viewName
            };

            var presentationMessage = new PresentationChangedMessage(presentationStep);

            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Publish(presentationMessage);
        }

        public virtual void UpdateNotifications()
        {
            this.ClearNotifications();
        }

        protected virtual Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            this.UpdateIsEnabled(this.machineModeService.MachinePower, e.MachineMode);

            return Task.CompletedTask;
        }

        protected virtual Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            this.UpdateIsEnabled(e.MachinePowerState, this.machineModeService.MachineMode);

            return Task.CompletedTask;
        }

        private void UpdateIsEnabled(MachinePowerState machinePower, MachineMode machineMode)
        {
            var enabeIfPoweredOn = (this.EnableMask & EnableMask.MachinePoweredOn) == EnableMask.MachinePoweredOn;

            var enableIfAutomatic = (this.EnableMask & EnableMask.MachineAutomaticMode) == EnableMask.MachineAutomaticMode;

            var enableIfManual = (this.EnableMask & EnableMask.MachineManualMode) == EnableMask.MachineManualMode;

            System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] POWER : {enabeIfPoweredOn && machinePower == MachinePowerState.Powered}");
            System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] AUTO  : {enableIfAutomatic && machineMode == MachineMode.Automatic}");
            System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] MANUAL: {enableIfManual && machineMode == MachineMode.Manual}");

            this.IsEnabled =
                this.EnableMask == EnableMask.Any
                ||
                (enabeIfPoweredOn && machinePower == MachinePowerState.Powered)
                ||
                (enableIfAutomatic && machineMode == MachineMode.Automatic)
                ||
                (enableIfManual && machineMode == MachineMode.Manual);
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
