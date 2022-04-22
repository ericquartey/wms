using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Modules;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Controls
{
    public abstract class BaseMainViewModel : BaseNavigationViewModel, IActivationViewModel, IRegionMemberLifetime
    {
        #region Fields

        protected bool isWaitingForResponse;

        private readonly IAuthenticationService authenticationService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IAuthenticationService>();

        private readonly IEventAggregator eventAggregator = CommonServiceLocator.ServiceLocator.Current.GetInstance<IEventAggregator>();

        private readonly IHealthProbeService healthProbeService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IHealthProbeService>();

        private readonly IInstallationHubClient installationHubClient = CommonServiceLocator.ServiceLocator.Current.GetInstance<IInstallationHubClient>();

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineCarouselWebService machineCarouselWebService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineCarouselWebService>();

        private readonly IMachineErrorsService machineErrorsService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineErrorsService>();

        private readonly IMachineModeService machineModeService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineModeService>();

        private readonly IMachineService machineService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineService>();

        private readonly ISensorsService sensorsService = CommonServiceLocator.ServiceLocator.Current.GetInstance<ISensorsService>();

        private readonly ISessionService sessionService = CommonServiceLocator.ServiceLocator.Current.GetInstance<ISessionService>();

        private SubscriptionToken bayChainHomingChangesToken;

        private SubscriptionToken bayChainPositionChangedToken;

        private DelegateCommand browserCloseCommand;

        private DelegateCommand browserOpenCommand;

        private SubscriptionToken healthStatusChangedToken;

        private SubscriptionToken homingChangesToken;

        private bool isBrowserOpened;

        private bool isEnabled;

        private bool isKeyboardButtonVisible;

        private bool isKeyboardOpened;

        private bool isWmsHealthy;

        private DelegateCommand keyboardCloseCommand;

        private DelegateCommand keyboardOpenCommand;

        private SubscriptionToken logoutChangesToken;

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

        public ICommand BrowserCloseCommand =>
                            this.browserCloseCommand
            ??
            (this.browserCloseCommand = new DelegateCommand(() => { this.IsBrowserOpened = false; }));

        public ICommand BrowserOpenCommand =>
                                    this.browserOpenCommand
            ??
            (this.browserOpenCommand = new DelegateCommand(() => { this.IsBrowserOpened = true; }));

        public virtual EnableMask EnableMask => EnableMask.MachinePoweredOn;

        public IEventAggregator EventAggregator => this.eventAggregator;

        public IHealthProbeService HealthProbeService => this.healthProbeService;

        public bool IsBrowserOpened
        {
            get => this.isBrowserOpened;
            set => this.SetProperty(ref this.isBrowserOpened, value, this.RaiseCanExecuteChanged);
        }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

        public bool IsKeyboardButtonVisible
        {
            get => this.isKeyboardButtonVisible;
            set
            {
                if (this.SetProperty(ref this.isKeyboardButtonVisible, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
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

        protected NLog.Logger Logger => this.logger;

        protected virtual bool RequireDataRefresh => true;

        #endregion

        #region Methods

        public static ObservableCollection<T> IEnumConvert<T>(IEnumerable<T> original)
        {
            return new ObservableCollection<T>(original);
        }

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

            if (this.logoutChangesToken != null)
            {
                this.EventAggregator?.GetEvent<PubSubEvent<LogoutMessageData>>().Unsubscribe(this.logoutChangesToken);
                this.logoutChangesToken?.Dispose();
                this.logoutChangesToken = null;
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

        public void SubscribeToBayEvent()
        {
            this.bayChainHomingChangesToken = this.bayChainHomingChangesToken
              ?? this.EventAggregator
                  .GetEvent<NotificationEventUI<HomingMessageData>>()
                  .Subscribe(
                      async (m) => await this.BayChainHomingAsync(m),
                      ThreadOption.UIThread,
                      false);
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

        private async Task BayChainHomingAsync(NotificationMessageUI<HomingMessageData> m)
        {
            if (m is null)
            {
                throw new ArgumentNullException(nameof(m));
            }

            if (m.Data.CalibrateFromPPC)
            {
                if (this.bayChainHomingChangesToken != null)
                {
                    this.EventAggregator?.GetEvent<PubSubEvent<HomingMessageData>>().Unsubscribe(this.bayChainHomingChangesToken);
                    this.bayChainHomingChangesToken?.Dispose();
                    this.bayChainHomingChangesToken = null;
                }
                Thread.Sleep(2000);
                await this.machineCarouselWebService.HomingAsync(true);
            }
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

        private async void OnLogoutMessageReceived(NotificationMessageUI<LogoutMessageData> message)
        {
            if (this.sessionService.UserAccessLevel <= UserAccessLevel.Movement)
            {
                this.logger.Debug($"Auto logout message processed");

                await this.authenticationService.LogOutAsync();

                this.NavigationService.Appear(
                    nameof(Login),
                    Login.LOGIN,
                    "NavigateToLoginPage");
            }
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

            this.logoutChangesToken = this.logoutChangesToken
              ?? this.EventAggregator
                  .GetEvent<NotificationEventUI<LogoutMessageData>>()
                  .Subscribe(
                      (m) => this.OnLogoutMessageReceived(m),
                      ThreadOption.UIThread,
                      false);
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
