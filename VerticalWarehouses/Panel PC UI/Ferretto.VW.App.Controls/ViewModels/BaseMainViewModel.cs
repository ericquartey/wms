using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Controls
{
    public abstract class BaseMainViewModel : BaseNavigationViewModel, IActivationViewModel, IRegionMemberLifetime
    {
        #region Fields

        protected bool isWaitingForResponse;

        private readonly IHealthProbeService healthProbeService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IHealthProbeService>();

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineErrorsService machineErrorsService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineErrorsService>();

        private readonly IMachineModeService machineModeService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineModeService>();

        private readonly IMachineService machineService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineService>();

        private SubscriptionToken bayChainPositionChangedToken;

        private SubscriptionToken healthStatusChangedToken;

        private SubscriptionToken homingChangesToken;

        private bool isEnabled;

        private bool isKeyboardOpened;

        private DelegateCommand keyboardCloseCommand;

        private DelegateCommand keyboardOpenCommand;

        private SubscriptionToken machineModeChangedToken;

        private SubscriptionToken machinePowerChangedToken;

        private SubscriptionToken machineStatusChangesToken;

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

        public IHealthProbeService HealthProbeService => this.healthProbeService;

        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

        public bool IsKeyboardOpened
        {
            get => this.isKeyboardOpened;
            set => this.SetProperty(ref this.isKeyboardOpened, value, this.RaiseCanExecuteChanged);
        }

        public virtual bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public virtual bool KeepAlive => true;

        public ICommand KeyboardCloseCommand =>
                            this.keyboardCloseCommand
            ??
            (this.keyboardCloseCommand = new DelegateCommand(() => this.KeyboardClose()));

        public ICommand KeyboardOpenCommand =>
           this.keyboardOpenCommand
           ??
           (this.keyboardOpenCommand = new DelegateCommand(() => this.KeyboardOpen()));

        protected NLog.Logger Logger => this.logger;

        public MachineError MachineError => this.machineErrorsService.ActiveError;

        public IMachineModeService MachineModeService => this.machineModeService;

        public IMachineService MachineService => this.machineService;

        public MachineStatus MachineStatus => this.MachineService.MachineStatus;

        public PresentationMode Mode
        {
            get => this.mode;
            set => this.SetProperty(ref this.mode, value);
        }

        protected bool IsConnectedByMAS => this.healthProbeService.HealthStatus == HealthStatus.Healthy;

        #endregion

        #region Methods

        public void ClearNotifications()
        {
            this.MachineService?.ClearNotifications();
        }

        public override void Disappear()
        {
            base.Disappear();

            this.IsWaitingForResponse = false;

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

        public virtual void InitializeSteps()
        {
            this.ShowPrevStep(false, false);
            this.ShowNextStep(false, false);
            this.ShowAbortStep(false, false);
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = false;

            this.UpdatePresentation();

            await this.machineService.OnUpdateServiceAsync();

            this.InitializeSteps();

            this.healthStatusChangedToken = this.healthStatusChangedToken
                ??
                this.EventAggregator
                    .GetEvent<PubSubEvent<HealthStatusChangedEventArgs>>()
                    .Subscribe(
                        async e => await this.OnHealthStatusChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

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

            this.bayChainPositionChangedToken = this.bayChainPositionChangedToken
                ??
                this.EventAggregator
                    .GetEvent<PubSubEvent<BayChainPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnBayChainPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.homingChangesToken = this.homingChangesToken
                ??
                this.EventAggregator
                    .GetEvent<HomingChangedPubSubEvent>()
                    .Subscribe(
                        (m) =>
                        {
                            this.UpdateIsEnabled(
                                this.machineModeService.MachinePower,
                                this.machineModeService.MachineMode,
                                this.healthProbeService.HealthStatus);
                        },
                        ThreadOption.UIThread,
                        false);

            this.machineStatusChangesToken = this.machineStatusChangesToken
                ?? this.EventAggregator
                    .GetEvent<MachineStatusChangedPubSubEvent>()
                    .Subscribe(
                        async (m) => await this.OnMachineStatusChangedAsync(m),
                        ThreadOption.UIThread,
                        false);

            this.machineErrorsService.ErrorStatusChanged += async (s, e) =>
            {
                await this.OnErrorStatusChangedAsync(e);
            };

            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                this.machineModeService.MachineMode,
                this.healthProbeService.HealthStatus);

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

        public void ShowNextStepSinglePage(bool isVisible, bool isEnabled, string moduleName = null, string viewName = null)
        {
            this.ShowStep(PresentationTypes.NextStep, isVisible, isEnabled, moduleName, viewName);
        }

        public void ShowNotification(string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            if (this.IsVisible)
            {
                this.EventAggregator
                 .GetEvent<PresentationNotificationPubSubEvent>()
                 .Publish(new PresentationNotificationMessage(message, severity));
            }
        }

        public void ShowNotification(Exception exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            this.Logger.Error(exception);

            if (this.IsVisible)
            {
                this.EventAggregator
                    .GetEvent<PresentationNotificationPubSubEvent>()
                    .Publish(new PresentationNotificationMessage(exception));
            }
        }

        public void ShowPrevStep(bool isVisible, bool isEnabled, string moduleName = null, string viewName = null)
        {
            this.ShowStep(PresentationTypes.Prev, isVisible, isEnabled, moduleName, viewName);
        }

        public void ShowPrevStepSinglePage(bool isVisible, bool isEnabled, string moduleName = null, string viewName = null)
        {
            this.ShowStep(PresentationTypes.PrevStep, isVisible, isEnabled, moduleName, viewName);
        }

        public void ShowStep(PresentationTypes presentationType, bool isVisible, bool isEnabled, string moduleName = null, string viewName = null)
        {
            if (this.IsVisible)
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
        }

        public virtual void UpdateNotifications()
        {
            this.ClearNotifications();
        }

        protected virtual Task OnErrorStatusChangedAsync(MachineErrorEventArgs e)
        {
            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                this.machineModeService.MachineMode,
                this.healthProbeService.HealthStatus);

            return Task.CompletedTask;
        }

        protected virtual Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                this.machineModeService.MachineMode,
                e.HealthStatus);

            return Task.CompletedTask;
        }

        protected virtual Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                e.MachineMode,
                this.healthProbeService.HealthStatus);

            return Task.CompletedTask;
        }

        protected virtual Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            this.UpdateIsEnabled(
                e.MachinePowerState,
                this.machineModeService.MachineMode,
                this.healthProbeService.HealthStatus);

            return Task.CompletedTask;
        }

        protected virtual Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                this.machineModeService.MachineMode,
                this.healthProbeService.HealthStatus);

            this.RaiseCanExecuteChanged();

            return Task.CompletedTask;
        }

        protected virtual void RaiseCanExecuteChanged()
        {
        }

        private void KeyboardClose()
        {
            this.IsKeyboardOpened = false;
        }

        private void KeyboardOpen()
        {
            this.IsKeyboardOpened = true;
        }

        private void OnBayChainPositionChanged(BayChainPositionChangedEventArgs e)
        {
            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                this.machineModeService.MachineMode,
                this.healthProbeService.HealthStatus);
        }

        private void UpdateIsEnabled(
            MachinePowerState machinePower,
            MachineMode machineMode,
            HealthStatus healthStatus)
        {
            var enabeIfPoweredOn = (this.EnableMask & EnableMask.MachinePoweredOn) == EnableMask.MachinePoweredOn;

            var enableIfAutomatic = (this.EnableMask & EnableMask.MachineAutomaticMode) == EnableMask.MachineAutomaticMode;

            var enableIfManual = (this.EnableMask & EnableMask.MachineManualMode) == EnableMask.MachineManualMode;

            this.IsEnabled =
                (this.EnableMask == EnableMask.Any) ||
                //
                (enabeIfPoweredOn &&
                 machinePower == MachinePowerState.Powered &&
                 healthStatus == HealthStatus.Healthy
                 //&& this.MachineError is null
                 ) ||
                //
                (enableIfAutomatic &&
                 machinePower == MachinePowerState.Powered &&
                 machineMode == MachineMode.Automatic
                 //&& this.MachineError is null
                 ) ||
                //
                (enableIfManual &&
                 machinePower == MachinePowerState.Powered &&
                 (machineMode == MachineMode.Manual || machineMode == MachineMode.Test)
                 //&& this.MachineError is null
                 );
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
