using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Controls
{
    public abstract class BaseMainViewModel : BaseNavigationViewModel, IActivationViewModel, IRegionMemberLifetime
    {
        #region Fields

        protected bool isWaitingForResponse;

        private readonly IEventAggregator eventAggregator = CommonServiceLocator.ServiceLocator.Current.GetInstance<IEventAggregator>();

        private readonly IHealthProbeService healthProbeService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IHealthProbeService>();

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineErrorsService machineErrorsService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineErrorsService>();

        private readonly IMachineModeService machineModeService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineModeService>();

        private readonly IMachineService machineService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineService>();

        private readonly ISensorsService sensorsService = CommonServiceLocator.ServiceLocator.Current.GetInstance<ISensorsService>();

        private SubscriptionToken bayChainPositionChangedToken;

        private SubscriptionToken healthStatusChangedToken;

        private SubscriptionToken homingChangesToken;

        private bool isEnabled;

        private bool isKeyboardOpened;

        private bool isWmsHealthy;

        private DelegateCommand keyboardCloseCommand;

        private DelegateCommand keyboardOpenCommand;

        private SubscriptionToken machineModeChangedToken;

        private SubscriptionToken machinePowerChangedToken;

        private SubscriptionToken machineStatusChangesToken;

        private PresentationMode mode;

        private SubscriptionToken sensorsToken;

        #endregion

        #region Constructors

        protected BaseMainViewModel(PresentationMode mode)
        {
            this.mode = mode;

            this.UpdateHealth();
        }

        #endregion

        #region Properties

        public virtual EnableMask EnableMask => EnableMask.MachinePoweredOn;

        public IEventAggregator EventAggregator => this.eventAggregator;

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

        public bool IsMachineMoving => this.machineService?.MachineStatus?.IsMoving ?? true;

        public bool IsMoving => (this.machineService?.MachineStatus?.IsMoving ?? true) || (this.machineService?.MachineStatus?.IsMovingLoadingUnit ?? true);

        public virtual bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public virtual bool IsWmsHealthy
        {
            get => this.isWmsHealthy;
            set => this.SetProperty(ref this.isWmsHealthy, value);
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

        public App.Services.Models.MachineStatus MachineStatus => this.machineService.MachineStatus;

        public PresentationMode Mode
        {
            get => this.mode;
            set => this.SetProperty(ref this.mode, value);
        }

        public ISensorsService SensorsService => this.sensorsService;

        protected bool IsConnectedByMAS => (this.healthProbeService.HealthMasStatus == HealthStatus.Healthy || this.healthProbeService.HealthMasStatus == HealthStatus.Degraded);

        protected virtual bool IsDataRefreshSyncronous => false;

        protected virtual bool RequireDataRefresh => true;

        #endregion

        #region Methods

        public void ClearNotifications()
        {
            this.MachineService?.ClearNotifications();
        }

        public void ClearSteps()
        {
            this.ShowPrevStepSinglePage(false, false);
            this.ShowNextStepSinglePage(false, false);
            this.ShowPrevStep(false, false);
            this.ShowNextStep(false, false);
            this.ShowAbortStep(false, false);
        }

        public override void Disappear()
        {
            base.Disappear();

            this.IsWaitingForResponse = false;

            this.sensorsToken?.Dispose();
            this.sensorsToken = null;

            if (this.machineStatusChangesToken != null)
            {
                this.EventAggregator?.GetEvent<MachineStatusChangedPubSubEvent>().Unsubscribe(this.machineStatusChangesToken);
                this.machineStatusChangesToken?.Dispose();
                this.machineStatusChangesToken = null;
            }

            if (this.machineModeChangedToken != null)
            {
                this.EventAggregator?.GetEvent<PubSubEvent<MachineModeChangedEventArgs>>().Unsubscribe(this.machineModeChangedToken);
                this.machineModeChangedToken?.Dispose();
                this.machineModeChangedToken = null;
            }

            //this.ClearSteps();

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
            this.IsWaitingForResponse = true;

            Task task = null;
            Task dataTask = null;
            try
            {
                if (!this.IsDataRefreshSyncronous)
                {
                    task = this.machineService.OnUpdateServiceAsync();
                    dataTask = this.OnDataRefreshAsync();
                }
                else
                {
                    await this.machineService.OnUpdateServiceAsync();
                    await this.OnDataRefreshAsync();
                    this.IsWaitingForResponse = false;
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
            }
            catch (Exception)
            {
                this.IsWaitingForResponse = false;
                this.RaiseCanExecuteChanged();
                throw;
            }

            this.SubscribeEvents();

            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                this.machineModeService.MachineMode,
                this.healthProbeService.HealthMasStatus);

            await base.OnAppearedAsync();

            try
            {
                if (!this.IsDataRefreshSyncronous)
                {
                    await task;
                    await dataTask;
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
            }
            catch (Exception)
            {
                this.IsWaitingForResponse = false;
                this.RaiseCanExecuteChanged();
                throw;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
            this.RaiseCanExecuteChanged();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
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

            this.Logger.Error(exception);

            this.EventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(exception));
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
            this.IsBackNavigationAllowed = !isVisible;

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

        protected virtual Task OnDataRefreshAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnErrorStatusChangedAsync(MachineErrorEventArgs e)
        {
            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                this.machineModeService.MachineMode,
                this.healthProbeService.HealthMasStatus);

            return Task.CompletedTask;
        }

        protected virtual Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                this.machineModeService.MachineMode,
                e.HealthMasStatus);

            this.IsWmsHealthy = e.HealthWmsStatus == HealthStatus.Healthy;

            this.RaiseCanExecuteChanged();

            return Task.CompletedTask;
        }

        protected virtual Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                e.MachineMode,
                this.healthProbeService.HealthMasStatus);

            this.RaiseCanExecuteChanged();

            return Task.CompletedTask;
        }

        protected virtual Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this.UpdateIsEnabled(
                e.MachinePowerState,
                this.machineModeService.MachineMode,
                this.healthProbeService.HealthMasStatus);

            this.RaiseCanExecuteChanged();

            return Task.CompletedTask;
        }

        protected virtual Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            this.UpdateIsEnabled(
                this.machineModeService.MachinePower,
                this.machineModeService.MachineMode,
                this.healthProbeService.HealthMasStatus);

            this.RaiseCanExecuteChanged();

            return Task.CompletedTask;
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            this.RaisePropertyChanged(nameof(this.IsMoving));
            this.RaisePropertyChanged(nameof(this.MachineService));
            this.RaisePropertyChanged(nameof(this.MachineStatus));
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
                this.healthProbeService.HealthMasStatus);
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            //this.RaiseCanExecuteChanged();
        }

        private void SubscribeEvents()
        {
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
                                this.healthProbeService.HealthMasStatus);
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

            this.sensorsToken = this.sensorsToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null &&
                             this.IsVisible);
        }

        private void UpdateHealth()
        {
            this.IsWmsHealthy = this.healthProbeService.HealthWmsStatus == HealthStatus.Healthy;
        }

        private void UpdateIsEnabled(
            MachinePowerState machinePower,
            MachineMode machineMode,
            HealthStatus healthStatus)
        {
            var result = true;
            if (this.EnableMask != EnableMask.Any)
            {
                foreach (EnableMask flag in Enum.GetValues(typeof(EnableMask)))
                {
                    if (this.EnableMask.HasFlag(flag))
                    {
                        switch (flag)
                        {
                            case EnableMask.MachinePoweredOff:
                                result &= machinePower == MachinePowerState.Unpowered;
                                break;

                            case EnableMask.MachinePoweredOn:
                                result &= machinePower == MachinePowerState.Powered;
                                break;

                            case EnableMask.MachineManualMode:
                                result &= (machineMode == MachineMode.Manual ||
                                    machineMode == MachineMode.Manual2 ||
                                    machineMode == MachineMode.Manual3 ||
                                    machineMode == MachineMode.Test ||
                                    machineMode == MachineMode.Test2 ||
                                    machineMode == MachineMode.Test3);
                                break;

                            case EnableMask.MachineAutomaticMode:
                                result &= (machineMode == MachineMode.Automatic ||
                                    machineMode == MachineMode.Compact ||
                                    machineMode == MachineMode.Compact2 ||
                                    machineMode == MachineMode.Compact3);
                                break;
                        }
                    }
                }
            }

            this.IsEnabled = result;
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
