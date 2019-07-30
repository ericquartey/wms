using System;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Installation.HelpWindows;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Installation.Resources;
using Ferretto.VW.App.Installation.Resources.Enumerables;
using Ferretto.VW.App.Installation.ViewsAndViewModels;
using Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation
{
    public delegate void ClickedOnMachineModeEvent();

    public delegate void ClickedOnMachineOnMarchEvent();

    public partial class MainWindowViewModel : BindableBase, IMainWindowViewModel
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly IEventAggregator eventAggregator;

        private readonly HelpMainWindow helpWindow;

        private readonly IdleViewModel idleViewModel;

        private readonly IInstallationHubClient installationHubClient;

        private BindableBase contentRegionCurrentViewModel;

        private BindableBase exitViewButtonRegionCurrentViewModel;

        private Visibility isErrorViewButtonVisible;

        private bool isExitViewButtonRegionExpanded;

        private Visibility isNavigationButtonRegionExpanded = Visibility.Visible;

        private bool isPopupOpen;

        private Visibility isServiceViewButtonVisible;

        private string loggedUser;

        private bool machineModeSelectionBool;

        private readonly bool machineOnMarchSelectionBool;

        private BindableBase navigationRegionCurrentViewModel;

        private ICommand openClosePopupCommand;

        private bool securityFunctionActive;

        private IUpdateSensorsService updateSensorsService;

        private IMachineStatusService machineStatusService;

        private IInverterStopService inverterStopService;

        #endregion

        #region Constructors

        public MainWindowViewModel(
            IEventAggregator eventAggregator,
            IInstallationHubClient installationHubClient,
            IMainWindowNavigationButtonsViewModel navigationButtonsViewModel,
            IIdleViewModel idleViewModel,
            IUnityContainer container)
        {
            this.eventAggregator = eventAggregator;
            this.container = container;
            this.installationHubClient = installationHubClient;
            this.NavigationRegionCurrentViewModel = navigationButtonsViewModel as MainWindowNavigationButtonsViewModel;
            this.ExitViewButtonRegionCurrentViewModel = null;
            this.idleViewModel = idleViewModel as IdleViewModel;
            this.ContentRegionCurrentViewModel = this.idleViewModel;
            this.SecurityFunctionActive = false;
            this.InitializeEvents();

            this.helpWindow = new HelpMainWindow(eventAggregator);
            this.IsErrorViewButtonVisible = Visibility.Collapsed;

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
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.ExitView));
        }));

        public ICommand BackToVWAPPCommand =>
            this.backToVWAPPCommand
            ??
            (this.backToVWAPPCommand = new DelegateCommand(() =>
                {
                    this.IsPopupOpen = false;
                    this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.BackToVWApp));
                    ClickedOnMachineModeEventHandler = null;
                    ClickedOnMachineOnMarchEventHandler = null;
                }));

        public BindableBase ContentRegionCurrentViewModel
        {
            get => this.contentRegionCurrentViewModel;
            set => this.SetProperty(ref this.contentRegionCurrentViewModel, value);
        }

        public BindableBase ExitViewButtonRegionCurrentViewModel { get => this.exitViewButtonRegionCurrentViewModel; set => this.SetProperty(ref this.exitViewButtonRegionCurrentViewModel, value); }

        public Visibility IsErrorViewButtonVisible { get => this.isErrorViewButtonVisible; set => this.SetProperty(ref this.isErrorViewButtonVisible, value); }

        public bool IsExitViewButtonRegionExpanded { get => this.isExitViewButtonRegionExpanded; set => this.SetProperty(ref this.isExitViewButtonRegionExpanded, value); }

        public Visibility IsNavigationButtonRegionExpanded { get => this.isNavigationButtonRegionExpanded; set => this.SetProperty(ref this.isNavigationButtonRegionExpanded, value); }

        public bool IsPopupOpen { get => this.isPopupOpen; set => this.SetProperty(ref this.isPopupOpen, value); }

        public Visibility IsServiceViewButtonVisible { get => this.isServiceViewButtonVisible; set => this.SetProperty(ref this.isServiceViewButtonVisible, value); }

        public string LoggedUser { get => this.loggedUser; set => this.SetProperty(ref this.loggedUser, value); }

        public ICommand MachineModeCustomCommand => this.machineModeCustomCommand ?? (this.machineModeCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineModeEvent()));

        public bool MachineModeSelectionBool { get => this.machineModeSelectionBool; set => this.SetProperty(ref this.machineModeSelectionBool, value); }

        public ICommand MachineOnMarchCustomCommand => this.machineOnMarchCustomCommand ?? (this.machineOnMarchCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineOnMarchEvent()));

        //public bool MachineOnMarchSelectionBool { get => this.machineOnMarchSelectionBool; set => this.SetProperty(ref this.machineOnMarchSelectionBool, value); }
        public bool MachineOnMarchSelectionBool { get => this.securityFunctionActive; set => this.SetProperty(ref this.securityFunctionActive, value); }

        public BindableBase NavigationRegionCurrentViewModel { get => this.navigationRegionCurrentViewModel; set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value); }

        public ICommand OpenClosePopupCommand => this.openClosePopupCommand ?? (this.openClosePopupCommand = new DelegateCommand(() => this.IsPopupOpen = !this.IsPopupOpen));

        public ICommand OpenHelpWindow => this.openHelpWindow ?? (this.openHelpWindow = new DelegateCommand(() =>
        {
            this.helpWindow.Show();
            this.helpWindow.HelpContentRegion.Content = this.contentRegionCurrentViewModel;
        }));

        public bool SecurityFunctionActive { get => this.securityFunctionActive; set => this.SetProperty(ref this.securityFunctionActive, value); }

        #endregion

        #region Methods

        private void InitializeEvents()
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) =>
                {
                    this.NavigationRegionCurrentViewModel = null;
                    this.ExitViewButtonRegionCurrentViewModel = (FooterViewModel)this.container.Resolve<IFooterViewModel>();
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.EnterView);

            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) =>
                {
                    this.NavigationRegionCurrentViewModel = (MainWindowNavigationButtonsViewModel)this.container.Resolve<IMainWindowNavigationButtonsViewModel>();
                    this.ExitViewButtonRegionCurrentViewModel = null;
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.ExitView);

            this.eventAggregator.GetEvent<MAS_ErrorEvent>().Subscribe(
                (message) =>
                {
                    this.IsErrorViewButtonVisible = Visibility.Visible;
                },
                ThreadOption.PublisherThread,
                false);

            this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                message => this.UpdateVariousInputsSensorsState(message.Data.SensorsStates),
                ThreadOption.PublisherThread,
                false);

            this.machineStatusService = this.container.Resolve<IMachineStatusService>();

            this.inverterStopService = this.container.Resolve<IInverterStopService>();

            MainWindow.FinishedMachineModeChangeStateEventHandler += () => { this.MachineModeSelectionBool = !this.MachineModeSelectionBool; };
            // TODO MachineOnMarch comes from the driver
            //MainWindow.FinishedMachineOnMarchChangeStateEventHandler += () => { this.MachineOnMarchSelectionBool = !this.MachineOnMarchSelectionBool; };
            ClickedOnMachineModeEventHandler += () => { };
            ClickedOnMachineOnMarchEventHandler += () => {
                if (!this.SecurityFunctionActive)
                {
                    this.machineStatusService.ExecuteResetSecurityAsync();
                }
                else
                {
                    this.inverterStopService.ExecuteAsync();
                }
            };

            this.updateSensorsService = this.container.Resolve<IUpdateSensorsService>();
            this.updateSensorsService.ExecuteAsync();

        }

        private void UpdateVariousInputsSensorsState(bool[] message)
        {
        }

        private void RaiseClickedOnMachineModeEvent() => ClickedOnMachineModeEventHandler();

        private void RaiseClickedOnMachineOnMarchEvent() => ClickedOnMachineOnMarchEventHandler();

        #endregion
    }
}
