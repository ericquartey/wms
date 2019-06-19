﻿using System.Windows;
using System.Windows.Input;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.Resources.Enumerables;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public delegate void ClickedOnMachineModeEvent();

    public delegate void ClickedOnMachineOnMarchEvent();

    public partial class MainWindowViewModel : BindableBase, IMainWindowViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly HelpMainWindow helpWindow;

        private readonly IInstallationHubClient installationHubClient;

        private IUnityContainer container;

        private BindableBase contentRegionCurrentViewModel;

        private BindableBase exitViewButtonRegionCurrentViewModel;

        private Visibility isErrorViewButtonVisible;

        private bool isExitViewButtonRegionExpanded;

        private Visibility isNavigationButtonRegionExpanded = Visibility.Visible;

        private bool isPopupOpen;

        private Visibility isServiceViewButtonVisible;

        private bool machineModeSelectionBool;

        private bool machineOnMarchSelectionBool;

        private BindableBase navigationRegionCurrentViewModel;

        private ICommand openClosePopupCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel(
            IEventAggregator eventAggregator,
            IInstallationHubClient installationHubClient)
        {
            this.eventAggregator = eventAggregator;
            this.helpWindow = new HelpMainWindow(eventAggregator);

            this.IsErrorViewButtonVisible = Visibility.Collapsed;

            this.installationHubClient = installationHubClient;
        }

        #endregion

        #region Events

        public static event ClickedOnMachineModeEvent ClickedOnMachineModeEventHandler;

        public static event ClickedOnMachineOnMarchEvent ClickedOnMachineOnMarchEventHandler;

        #endregion

        #region Properties

        public ICommand BackToMainWindowNavigationButtonsViewButtonCommand => this.backToMainWindowNavigationButtonsViewCommand ?? (this.backToMainWindowNavigationButtonsViewCommand = new DelegateCommand(() =>
        {
            this.NavigationRegionCurrentViewModel = (MainWindowNavigationButtonsViewModel)this.container.Resolve<IMainWindowNavigationButtonsViewModel>();
            this.ContentRegionCurrentViewModel = (IdleViewModel)this.container.Resolve<IIdleViewModel>();
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.ExitView));
        }));

        public ICommand BackToVWAPPCommand => this.backToVWAPPCommand ?? (this.backToVWAPPCommand = new DelegateCommand(() =>
        {
            this.IsPopupOpen = false;
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.BackToVWApp));
            ClickedOnMachineModeEventHandler = null;
            ClickedOnMachineOnMarchEventHandler = null;
        }));

        public BindableBase ContentRegionCurrentViewModel { get => this.contentRegionCurrentViewModel; set => this.SetProperty(ref this.contentRegionCurrentViewModel, value); }

        public BindableBase ExitViewButtonRegionCurrentViewModel { get => this.exitViewButtonRegionCurrentViewModel; set => this.SetProperty(ref this.exitViewButtonRegionCurrentViewModel, value); }

        public Visibility IsErrorViewButtonVisible { get => this.isErrorViewButtonVisible; set => this.SetProperty(ref this.isErrorViewButtonVisible, value); }

        public bool IsExitViewButtonRegionExpanded { get => this.isExitViewButtonRegionExpanded; set => this.SetProperty(ref this.isExitViewButtonRegionExpanded, value); }

        public Visibility IsNavigationButtonRegionExpanded { get => this.isNavigationButtonRegionExpanded; set => this.SetProperty(ref this.isNavigationButtonRegionExpanded, value); }

        public bool IsPopupOpen { get => this.isPopupOpen; set => this.SetProperty(ref this.isPopupOpen, value); }

        public Visibility IsServiceViewButtonVisible { get => this.isServiceViewButtonVisible; set => this.SetProperty(ref this.isServiceViewButtonVisible, value); }

        public ICommand MachineModeCustomCommand => this.machineModeCustomCommand ?? (this.machineModeCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineModeEvent()));

        public bool MachineModeSelectionBool { get => this.machineModeSelectionBool; set => this.SetProperty(ref this.machineModeSelectionBool, value); }

        public ICommand MachineOnMarchCustomCommand => this.machineOnMarchCustomCommand ?? (this.machineOnMarchCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineOnMarchEvent()));

        public bool MachineOnMarchSelectionBool { get => this.machineOnMarchSelectionBool; set => this.SetProperty(ref this.machineOnMarchSelectionBool, value); }

        public BindableBase NavigationRegionCurrentViewModel { get => this.navigationRegionCurrentViewModel; set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value); }

        public ICommand OpenClosePopupCommand => this.openClosePopupCommand ?? (this.openClosePopupCommand = new DelegateCommand(() => this.IsPopupOpen = !this.IsPopupOpen));

        public ICommand OpenHelpWindow => this.openHelpWindow ?? (this.openHelpWindow = new DelegateCommand(() =>
        {
            this.helpWindow.Show();
            this.helpWindow.HelpContentRegion.Content = this.contentRegionCurrentViewModel;
        }));

        #endregion

        #region Methods

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.NavigationRegionCurrentViewModel = (MainWindowNavigationButtonsViewModel)this.container.Resolve<IMainWindowNavigationButtonsViewModel>();
            this.ExitViewButtonRegionCurrentViewModel = null;
            this.ContentRegionCurrentViewModel = (IdleViewModel)this.container.Resolve<IIdleViewModel>();
            this.InitializeEvents();
        }

        private void InitializeEvents()
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) =>
            {
                this.NavigationRegionCurrentViewModel = null;
                this.ExitViewButtonRegionCurrentViewModel = (MainWindowBackToIAPPButtonViewModel)this.container.Resolve<IMainWindowBackToIAPPButtonViewModel>();
                ((MainWindowBackToIAPPButtonViewModel)this.container.Resolve<IMainWindowBackToIAPPButtonViewModel>()).InitializeBottomButtons();
            },
            ThreadOption.PublisherThread,
            false,
            message => message.Type == InstallationApp_EventMessageType.EnterView);

            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe((message) =>
            {
                this.NavigationRegionCurrentViewModel = (MainWindowNavigationButtonsViewModel)this.container.Resolve<IMainWindowNavigationButtonsViewModel>();
                this.ExitViewButtonRegionCurrentViewModel = null;
                ((MainWindowBackToIAPPButtonViewModel)this.container.Resolve<IMainWindowBackToIAPPButtonViewModel>()).FinalizeBottomButtons();
            },
            ThreadOption.PublisherThread,
            false,
            message => message.Type == InstallationApp_EventMessageType.ExitView);

            this.eventAggregator.GetEvent<MAS_ErrorEvent>().Subscribe((message) =>
            {
                this.IsErrorViewButtonVisible = Visibility.Visible;
            },
            ThreadOption.PublisherThread,
            false);

            MainWindow.FinishedMachineModeChangeStateEventHandler += () => { this.MachineModeSelectionBool = !this.MachineModeSelectionBool; };
            MainWindow.FinishedMachineOnMarchChangeStateEventHandler += () => { this.MachineOnMarchSelectionBool = !this.MachineOnMarchSelectionBool; };
            ClickedOnMachineModeEventHandler += () => { };
            ClickedOnMachineOnMarchEventHandler += () => { };
        }

        private void RaiseClickedOnMachineModeEvent() => ClickedOnMachineModeEventHandler();

        private void RaiseClickedOnMachineOnMarchEvent() => ClickedOnMachineOnMarchEventHandler();

        #endregion
    }
}
