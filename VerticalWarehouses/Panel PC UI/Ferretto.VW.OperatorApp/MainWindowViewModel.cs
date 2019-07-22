using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.Resources;
using Ferretto.VW.OperatorApp.Resources.Enumerations;
using Ferretto.VW.OperatorApp.ViewsAndViewModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp
{
    public delegate void ClickedOnMachineModeEvent();

    public delegate void ClickedOnMachineOnMarchEvent();

    public class MainWindowViewModel : BindableBase, IMainWindowViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly HelpMainWindow helpWindow;

        private ICommand backToVWAPPCommand;

        private BindableBase contentRegionCurrentViewModel;

        private BindableBase exitViewButtonRegionCurrentViewModel;

        private bool isPopupOpen;

        private string loggedUser;

        private bool machineModeSelectionBool;

        private bool machineOnMarchSelectionBool;

        private BindableBase navigationRegionCurrentViewModel;

        private ICommand openClosePopupCommand;

        private ICommand openHelpWindow;

        #endregion

        #region Constructors

        public MainWindowViewModel(
            IEventAggregator eventAggregator,
            IMainWindowNavigationButtonsViewModel navigationButtonsViewModel,
            IIdleViewModel idleViewModel,
            IAuthenticationService authenticationService)
        {
            if (authenticationService == null)
            {
                throw new System.ArgumentNullException(nameof(authenticationService));
            }

            this.eventAggregator = eventAggregator;
            this.NavigationRegionCurrentViewModel = navigationButtonsViewModel as MainWindowNavigationButtonsViewModel;
            this.ExitViewButtonRegionCurrentViewModel = null;
            this.ContentRegionCurrentViewModel = (IdleViewModel)idleViewModel;

            authenticationService.UserAuthenticated += this.AuthenticationService_UserAuthenticated;
            this.LoggedUser = authenticationService.UserName;

            this.InitializeEvents();

            this.helpWindow = new HelpMainWindow(eventAggregator);
        }

        #endregion

        #region Events

        public static event ClickedOnMachineModeEvent ClickedOnMachineModeEventHandler;

        public static event ClickedOnMachineOnMarchEvent ClickedOnMachineOnMarchEventHandler;

        #endregion

        #region Properties

        public ICommand BackToVWAPPCommand => this.backToVWAPPCommand ?? (this.backToVWAPPCommand = new DelegateCommand(() =>
        {
            this.IsPopupOpen = false;
            this.eventAggregator.GetEvent<OperatorApp_Event>().Publish(new OperatorApp_EventMessage(OperatorApp_EventMessageType.BackToVWApp));
            ClickedOnMachineModeEventHandler = null;
            ClickedOnMachineOnMarchEventHandler = null;
        }));

        public BindableBase ContentRegionCurrentViewModel { get => this.contentRegionCurrentViewModel; set => this.SetProperty(ref this.contentRegionCurrentViewModel, value); }

        public BindableBase ExitViewButtonRegionCurrentViewModel { get => this.exitViewButtonRegionCurrentViewModel; set => this.SetProperty(ref this.exitViewButtonRegionCurrentViewModel, value); }

        public bool IsPopupOpen { get => this.isPopupOpen; set => this.SetProperty(ref this.isPopupOpen, value); }

        public string LoggedUser
        {
            get => this.loggedUser;
            set => this.SetProperty(ref this.loggedUser, value);
        }

        public bool MachineModeSelectionBool { get => this.machineModeSelectionBool; set => this.SetProperty(ref this.machineModeSelectionBool, value); }

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

        private static void RaiseClickedOnMachineModeEvent() => ClickedOnMachineModeEventHandler();

        private static void RaiseClickedOnMachineOnMarchEvent() => ClickedOnMachineOnMarchEventHandler();

        private void AuthenticationService_UserAuthenticated(object sender, UserAuthenticatedEventArgs e)
        {
            this.LoggedUser = e.UserName;
        }

        private void InitializeEvents()
        {
            MainWindow.FinishedMachineModeChangeStateEventHandler += () => { this.MachineModeSelectionBool = !this.MachineModeSelectionBool; };
            MainWindow.FinishedMachineOnMarchChangeStateEventHandler += () => { this.MachineOnMarchSelectionBool = !this.MachineOnMarchSelectionBool; };
            ClickedOnMachineModeEventHandler += () => { };
            ClickedOnMachineOnMarchEventHandler += () => { };

            this.eventAggregator.GetEvent<NotificationEventUI<ExecuteMissionMessageData>>().Subscribe(
                message => this.OnCurrentMissionChanged(message.Data.Mission, message.Data.PendingMissionsCount));
        }

        private void OnCurrentMissionChanged(MissionInfo mission, int missionsQuantity)
        {
            if (this.ContentRegionCurrentViewModel is IDrawerActivityViewModel content)
            {
                content.UpdateView();
            }
        }

        #endregion
    }
}
