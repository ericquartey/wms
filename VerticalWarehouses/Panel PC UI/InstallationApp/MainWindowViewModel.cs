using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Views.ErrorDetails;
using Ferretto.VW.App.Installation.HelpWindows;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Installation.Resources;
using Ferretto.VW.App.Installation.Resources.Enumerables;
using Ferretto.VW.App.Installation.ViewsAndViewModels;
using Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Enumerations;

namespace Ferretto.VW.App.Installation
{
    public delegate void ClickedOnMachineModeEvent();

    public delegate void ClickedOnMachineOnMarchEvent();

    public partial class MainWindowViewModel : BaseViewModel, IMainWindowViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IUnityContainer container;

        private readonly IErrorsMachineService errorsMachineService;

        private readonly IEventAggregator eventAggregator;

        private readonly HelpMainWindow helpWindow;

        private readonly IdleViewModel idleViewModel;

        private readonly bool machineOnMarchSelectionBool;

        private readonly IOperatorHubClient operatorHubClient;

        private readonly IStatusMessageService statusMessageService;

        private IViewModel contentRegionCurrentViewModel;

        private BindableBase exitViewButtonRegionCurrentViewModel;

        private bool isExitViewButtonRegionExpanded;

        private Visibility isNavigationButtonRegionExpanded = Visibility.Visible;

        private bool isPopupOpen;

        private Visibility isServiceViewButtonVisible;

        private string loggedUser;

        private bool machineHasErrors;

        private bool machineModeSelectionBool;

        private IMachineStatusMachineService machineStatusService;

        private IViewModel navigationRegionCurrentViewModel;

        private ICommand openClosePopupCommand;

        private IViewModel previousContentRegionViewModel;

        private IViewModel previousNavigationViewModel;

        private bool securityFunctionActive;

        private ISensorsMachineService sensorsMachineService;

        private ICommand showErrorDetailsCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel(
            IEventAggregator eventAggregator,
            IMainWindowNavigationButtonsViewModel navigationButtonsViewModel,
            IIdleViewModel idleViewModel,
            IUnityContainer container,
            IErrorsMachineService errorsMachineService,
            IAuthenticationService authenticationService,
            IOperatorHubClient operatorHubClient,
            IStatusMessageService statusMessageService)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (navigationButtonsViewModel == null)
            {
                throw new System.ArgumentNullException(nameof(navigationButtonsViewModel));
            }

            if (idleViewModel == null)
            {
                throw new System.ArgumentNullException(nameof(idleViewModel));
            }

            if (container == null)
            {
                throw new System.ArgumentNullException(nameof(container));
            }

            if (errorsMachineService == null)
            {
                throw new System.ArgumentNullException(nameof(errorsMachineService));
            }

            if (authenticationService == null)
            {
                throw new System.ArgumentNullException(nameof(authenticationService));
            }

            if (operatorHubClient == null)
            {
                throw new System.ArgumentNullException(nameof(operatorHubClient));
            }

            if (statusMessageService == null)
            {
                throw new System.ArgumentNullException(nameof(statusMessageService));
            }

            this.eventAggregator = eventAggregator;
            this.container = container;
            this.errorsMachineService = errorsMachineService;
            this.authenticationService = authenticationService;
            this.operatorHubClient = operatorHubClient;
            this.statusMessageService = statusMessageService;
            this.NavigationRegionCurrentViewModel = navigationButtonsViewModel as MainWindowNavigationButtonsViewModel;
            this.ExitViewButtonRegionCurrentViewModel = null;
            this.idleViewModel = idleViewModel as IdleViewModel;
            this.ContentRegionCurrentViewModel = this.idleViewModel;
            this.SecurityFunctionActive = false;
            this.InitializeEvents();

            this.helpWindow = new HelpMainWindow(eventAggregator);

            this.operatorHubClient.ErrorStatusChanged += async (sender, e) => await this.OnMachineErrorStatusChanged(sender, e);
            authenticationService.UserAuthenticated += this.OnUserAuthenticated;
            this.LoggedUser = authenticationService.UserName;
        }

        #endregion

        #region Events

        public static event ClickedOnMachineModeEvent ClickedOnMachineModeEventHandler;

        public static event ClickedOnMachineOnMarchEvent ClickedOnMachineOnMarchEventHandler;

        #endregion

        #region Properties

        public ICommand BackToMainWindowNavigationButtonsViewButtonCommand =>
            this.backToMainWindowNavigationButtonsViewCommand
            ??
            (this.backToMainWindowNavigationButtonsViewCommand = new DelegateCommand(() =>
        {
            this.NavigationRegionCurrentViewModel = (MainWindowNavigationButtonsViewModel)this.container.Resolve<IMainWindowNavigationButtonsViewModel>();
            this.ContentRegionCurrentViewModel = this.idleViewModel;
            this.eventAggregator
                .GetEvent<InstallationApp_Event>()
                .Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.ExitView));
        }));

        public ICommand BackToVWAPPCommand =>
            this.backToVWAPPCommand
            ??
            (this.backToVWAPPCommand = new DelegateCommand(() =>
                {
                    this.IsPopupOpen = false;
                    this.eventAggregator
                        .GetEvent<InstallationApp_Event>()
                        .Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.BackToVWApp));
                    //ClickedOnMachineModeEventHandler = null;
                    //ClickedOnMachineOnMarchEventHandler = null;
                }));

        public IViewModel ContentRegionCurrentViewModel
        {
            get => this.contentRegionCurrentViewModel;
            set => this.SetProperty(ref this.contentRegionCurrentViewModel, value);
        }

        public BindableBase ExitViewButtonRegionCurrentViewModel
        {
            get => this.exitViewButtonRegionCurrentViewModel;
            set => this.SetProperty(ref this.exitViewButtonRegionCurrentViewModel, value);
        }

        public bool IsExitViewButtonRegionExpanded
        {
            get => this.isExitViewButtonRegionExpanded;
            set => this.SetProperty(ref this.isExitViewButtonRegionExpanded, value);
        }

        public Visibility IsNavigationButtonRegionExpanded
        {
            get => this.isNavigationButtonRegionExpanded;
            set => this.SetProperty(ref this.isNavigationButtonRegionExpanded, value);
        }

        public bool IsPopupOpen
        {
            get => this.isPopupOpen;
            set => this.SetProperty(ref this.isPopupOpen, value);
        }

        public Visibility IsServiceViewButtonVisible
        {
            get => this.isServiceViewButtonVisible;
            set => this.SetProperty(ref this.isServiceViewButtonVisible, value);
        }

        public string LoggedUser
        {
            get => this.loggedUser;
            set => this.SetProperty(ref this.loggedUser, value);
        }

        public bool MachineHasErrors
        {
            get => this.machineHasErrors;
            set => this.SetProperty(ref this.machineHasErrors, value);
        }

        public ICommand MachineModeCustomCommand =>
            this.machineModeCustomCommand
            ??
            (this.machineModeCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineModeEvent()));

        public bool MachineModeSelectionBool
        {
            get => this.machineModeSelectionBool;
            set => this.SetProperty(ref this.machineModeSelectionBool, value);
        }

        public ICommand MachineOnMarchCustomCommand =>
            this.machineOnMarchCustomCommand
            ??
            (this.machineOnMarchCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineOnMarchEvent()));

        public bool MachineOnMarchSelectionBool
        {
            get => this.securityFunctionActive;
            set => this.SetProperty(ref this.securityFunctionActive, value);
        }

        public IViewModel NavigationRegionCurrentViewModel
        {
            get => this.navigationRegionCurrentViewModel;
            set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value);
        }

        public ICommand OpenClosePopupCommand =>
            this.openClosePopupCommand
            ??
            (this.openClosePopupCommand = new DelegateCommand(() => this.IsPopupOpen = !this.IsPopupOpen));

        public ICommand OpenHelpWindow =>
            this.openHelpWindow
            ??
            (this.openHelpWindow = new DelegateCommand(() =>
            {
                this.helpWindow.Show();
                this.helpWindow.HelpContentRegion.Content = this.contentRegionCurrentViewModel;
            }));

        public bool SecurityFunctionActive { get => this.securityFunctionActive; set => this.SetProperty(ref this.securityFunctionActive, value); }

        public ICommand ShowErrorDetailsCommand =>
            this.showErrorDetailsCommand
            ??
            (this.showErrorDetailsCommand = new DelegateCommand(async () => await this.ExecuteShowErrorDetailsCommandAsync()));

        #endregion

        #region Methods

        public override async Task OnEnterViewAsync()
        {
            var error = await this.errorsMachineService.GetCurrentAsync();
            this.MachineHasErrors = error != null;
        }

        private async Task ExecuteShowErrorDetailsCommandAsync()
        {
            var errorDetailsViewModel = this.container.Resolve<ErrorDetailsViewModel>();

            if (this.ContentRegionCurrentViewModel != errorDetailsViewModel)
            // navigate to error page
            {
                try
                {
                    errorDetailsViewModel.Error = await this.errorsMachineService.GetCurrentAsync();

                    this.previousNavigationViewModel = this.NavigationRegionCurrentViewModel;
                    this.previousContentRegionViewModel = this.ContentRegionCurrentViewModel;

                    this.NavigationRegionCurrentViewModel = null;
                    this.ContentRegionCurrentViewModel = errorDetailsViewModel;

                    var footerViewModel = this.container.Resolve<IFooterViewModel>();
                }
                catch (System.Exception ex)
                {
                    this.statusMessageService.Notify(ex);
                }
            }
            else
            // leave error page
            {
                this.NavigationRegionCurrentViewModel = this.previousNavigationViewModel;
                this.ContentRegionCurrentViewModel = this.previousContentRegionViewModel;

                this.previousNavigationViewModel = null;
                this.previousContentRegionViewModel = null;
            }
        }

        private void InitializeEvents()
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) =>
                {
                    this.NavigationRegionCurrentViewModel = null;
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.EnterView);

            this.eventAggregator.GetEvent<InstallationApp_Event>()
                .Subscribe(
                (message) =>
                {
                    this.NavigationRegionCurrentViewModel = this.container.Resolve<IMainWindowNavigationButtonsViewModel>();
                    this.ExitViewButtonRegionCurrentViewModel = null;
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.ExitView);

            this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                message => this.UpdateVariousInputsSensorsState(message.Data.SensorsStates),
                ThreadOption.PublisherThread,
                false);

            this.machineStatusService = this.container.Resolve<IMachineStatusMachineService>();

            MainWindow.FinishedMachineModeChangeStateEventHandler += () => { this.MachineModeSelectionBool = !this.MachineModeSelectionBool; };
            // TODO MachineOnMarch comes from the driver
            //MainWindow.FinishedMachineOnMarchChangeStateEventHandler += () => { this.MachineOnMarchSelectionBool = !this.MachineOnMarchSelectionBool; };
            ClickedOnMachineModeEventHandler += () => { };
            ClickedOnMachineOnMarchEventHandler += () =>
            {
                if (!this.SecurityFunctionActive)
                {
                    this.machineStatusService.ExecuteResetSecurityAsync();
                    //this.securityFunctionActive = true;     // TODO - remove this line when this value comes from IoDriver
                }
            };

            this.updateSensorsService = this.container.Resolve<IUpdateSensorsMachineService>();
            this.updateSensorsService.ExecuteAsync();
        }

        private async Task OnMachineErrorStatusChanged(object sender,
            MAS.AutomationService.Contracts.Hubs.EventArgs.ErrorStatusChangedEventArgs e)
        {
            var error = await this.errorsMachineService.GetCurrentAsync();

            this.MachineHasErrors = error != null;
        }

        private void OnUserAuthenticated(object sender, Services.UserAuthenticatedEventArgs e)
        {
            this.LoggedUser = e.UserName;
        }

        private void RaiseClickedOnMachineModeEvent() => ClickedOnMachineModeEventHandler();

        private void RaiseClickedOnMachineOnMarchEvent() => ClickedOnMachineOnMarchEventHandler();

        private void UpdateVariousInputsSensorsState(bool[] message)
        {
            this.MachineOnMarchSelectionBool = message[(int)IOMachineSensors.NormalState];
        }

        #endregion
    }
}
