using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.Resources.Enumerables;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public delegate void ClickedOnMachineModeEvent();

    public delegate void ClickedOnMachineOnMarchEvent();

    public delegate void SensorsStatesChangedEvent();

    public partial class MainWindowViewModel : BindableBase, IMainWindowViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private readonly HelpMainWindow helpWindow;

        private IUnityContainer container;

        private BindableBase contentRegionCurrentViewModel;

        private IEventAggregator eventAggregator;

        private BindableBase exitViewButtonRegionCurrentViewModel;

        private string internalMessages;

        private bool isExitViewButtonRegionExpanded;

        private Visibility isNavigationButtonRegionExpanded = Visibility.Visible;

        private bool isPopupOpen;

        private bool machineModeSelectionBool;

        private bool machineOnMarchSelectionBool;

        private string messageFromServer;

        private BindableBase navigationRegionCurrentViewModel;

        private ICommand openClosePopupCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.helpWindow = new HelpMainWindow(eventAggregator);
        }

        #endregion

        #region Events

        public static event ClickedOnMachineModeEvent ClickedOnMachineModeEventHandler;

        public static event ClickedOnMachineOnMarchEvent ClickedOnMachineOnMarchEventHandler;

        #endregion

        #region Properties

        public BindableBase ContentRegionCurrentViewModel { get => this.contentRegionCurrentViewModel; set => this.SetProperty(ref this.contentRegionCurrentViewModel, value); }

        public BindableBase ExitViewButtonRegionCurrentViewModel { get => this.exitViewButtonRegionCurrentViewModel; set => this.SetProperty(ref this.exitViewButtonRegionCurrentViewModel, value); }

        public string InternalMessages { get => this.internalMessages; set => this.internalMessages = value; }

        public bool IsExitViewButtonRegionExpanded { get => this.isExitViewButtonRegionExpanded; set => this.SetProperty(ref this.isExitViewButtonRegionExpanded, value); }

        public Visibility IsNavigationButtonRegionExpanded { get => this.isNavigationButtonRegionExpanded; set => this.SetProperty(ref this.isNavigationButtonRegionExpanded, value); }

        public bool IsPopupOpen { get => this.isPopupOpen; set => this.SetProperty(ref this.isPopupOpen, value); }

        public bool MachineModeSelectionBool { get => this.machineModeSelectionBool; set => this.SetProperty(ref this.machineModeSelectionBool, value); }

        public bool MachineOnMarchSelectionBool { get => this.machineOnMarchSelectionBool; set => this.SetProperty(ref this.machineOnMarchSelectionBool, value); }

        public string MessageFromServer { get => this.messageFromServer; set => this.messageFromServer = value; }

        public BindableBase NavigationRegionCurrentViewModel { get => this.navigationRegionCurrentViewModel; set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value); }

        public ICommand OpenClosePopupCommand => this.openClosePopupCommand ?? (this.openClosePopupCommand = new DelegateCommand(() => this.IsPopupOpen = !this.IsPopupOpen));

        #endregion

        #region Methods

        public string Get(string uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.container = _container;
            this.NavigationRegionCurrentViewModel = (MainWindowNavigationButtonsViewModel)this.container.Resolve<IMainWindowNavigationButtonsViewModel>();
            this.ExitViewButtonRegionCurrentViewModel = null;
            this.ContentRegionCurrentViewModel = (IdleViewModel)this.container.Resolve<IIdleViewModel>();
            this.InitializeEvents();
        }

        private void InitializeEvents()
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe((message) =>
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
