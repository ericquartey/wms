using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.Navigation;
using Ferretto.VW.Utils.Source;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;

#if CODEMAID
    // disable codemaid region messer in this file
#endif

namespace Ferretto.VW.InstallationApp
{
    public delegate void ClickedOnMachineOnMarchEvent();

    public delegate void ClickedOnMachineModeEvent();

    public delegate void SensorsStatesChangedEvent();

    public partial class MainWindowViewModel : BindableBase, IMainWindowViewModel, IViewModelRequiresContainer
    {
        #region Constants, Statics & Others

        private static readonly string SENSOR_INITIALIZER_URL = ConfigurationManager.AppSettings["SensorsStatesInitializer"];

        private static readonly string SERVICE_PATH = ConfigurationManager.AppSettings["SensorsStatesHubPath"];

        private static readonly string URL = ConfigurationManager.AppSettings["ServiceURL"];

        public static SensorsStates States;

        private InstallationHubClient installationClient;

        private SensorsStatesHubClient sensorsClient;

        private BindableBase contentRegionCurrentViewModel;

        private BindableBase navigationRegionCurrentViewModel;

        private BindableBase exitViewButtonRegionCurrentViewModel;

        private bool machineModeSelectionBool = false;

        private bool machineOnMarchSelectionBool = false;

        private Visibility isNavigationButtonRegionExpanded = Visibility.Visible;

        private bool isExitViewButtonRegionExpanded = false;

        private bool isPopupOpen = false;

        private ICommand openClosePopupCommand;

        public IUnityContainer Container;

        private readonly HelpMainWindow helpWindow = new HelpMainWindow();

        private string internalMessages;

        private string messageFromServer;

        #endregion Constants, Statics & Others

        #region Events

        public static event SensorsStatesChangedEvent SensorsStatesChangedEventHandler;

        public static event ClickedOnMachineOnMarchEvent ClickedOnMachineOnMarchEventHandler;

        public static event ClickedOnMachineModeEvent ClickedOnMachineModeEventHandler;

        #endregion Events

        #region Other Properties

        public BindableBase ContentRegionCurrentViewModel { get => this.contentRegionCurrentViewModel; set => this.SetProperty(ref this.contentRegionCurrentViewModel, value); }

        public bool MachineModeSelectionBool { get => this.machineModeSelectionBool; set => this.SetProperty(ref this.machineModeSelectionBool, value); }

        public bool MachineOnMarchSelectionBool { get => this.machineOnMarchSelectionBool; set => this.SetProperty(ref this.machineOnMarchSelectionBool, value); }

        public BindableBase NavigationRegionCurrentViewModel { get => this.navigationRegionCurrentViewModel; set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value); }

        public Visibility IsNavigationButtonRegionExpanded { get => this.isNavigationButtonRegionExpanded; set => this.SetProperty(ref this.isNavigationButtonRegionExpanded, value); }

        public BindableBase ExitViewButtonRegionCurrentViewModel { get => this.exitViewButtonRegionCurrentViewModel; set => this.SetProperty(ref this.exitViewButtonRegionCurrentViewModel, value); }

        public bool IsExitViewButtonRegionExpanded { get => this.isExitViewButtonRegionExpanded; set => this.SetProperty(ref this.isExitViewButtonRegionExpanded, value); }

        public bool IsPopupOpen { get => this.isPopupOpen; set => this.SetProperty(ref this.isPopupOpen, value); }

        public ICommand OpenClosePopupCommand => this.openClosePopupCommand ?? (this.openClosePopupCommand = new DelegateCommand(() => this.IsPopupOpen = !this.IsPopupOpen));

        public string InternalMessages { get => this.internalMessages; set => this.internalMessages = value; }

        public string MessageFromServer { get => this.messageFromServer; set => this.messageFromServer = value; }

        #endregion Other Properties

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
            return "";
        }

        private void Client_SensorsStatesChanged(object sender, SensorsStatesEventArgs e)
        {
            States = e.SensorsStates;
            this.RaiseSensorsStatesChangedEvent();
        }

        private async void ConnectMethod()
        {
            try
            {
                this.sensorsClient = new SensorsStatesHubClient(URL, SERVICE_PATH);
                await this.sensorsClient.ConnectAsync();

                this.sensorsClient.SensorsStatesChanged += this.Client_SensorsStatesChanged;
                this.Get(SENSOR_INITIALIZER_URL);
            }
            catch
            {
                // TODO
            }
        }

        private void InitializeEvents()
        {
            NavigationService.GoToViewEventHandler += () => this.NavigationRegionCurrentViewModel = null;
            NavigationService.GoToViewEventHandler += () => this.ExitViewButtonRegionCurrentViewModel = (MainWindowBackToIAPPButtonViewModel)this.Container.Resolve<IMainWindowBackToIAPPButtonViewModel>();
            NavigationService.GoToViewEventHandler += () => ((MainWindowBackToIAPPButtonViewModel)this.Container.Resolve<IMainWindowBackToIAPPButtonViewModel>()).InitializeBottomButtons();
            NavigationService.ExitViewEventHandler += () => this.NavigationRegionCurrentViewModel = (MainWindowNavigationButtonsViewModel)this.Container.Resolve<IMainWindowNavigationButtonsViewModel>();
            NavigationService.ExitViewEventHandler += () => this.ExitViewButtonRegionCurrentViewModel = null;
            NavigationService.ExitViewEventHandler += () => ((MainWindowBackToIAPPButtonViewModel)this.Container.Resolve<IMainWindowBackToIAPPButtonViewModel>()).FinalizeBottomButtons();
            MainWindow.FinishedMachineModeChangeStateEventHandler += () => { this.MachineModeSelectionBool = !this.MachineModeSelectionBool; };
            MainWindow.FinishedMachineOnMarchChangeStateEventHandler += () => { this.MachineOnMarchSelectionBool = !this.MachineOnMarchSelectionBool; };
            ClickedOnMachineModeEventHandler += () => { };
            ClickedOnMachineOnMarchEventHandler += () => { };
            SensorsStatesChangedEventHandler += () => { };
        }

        private void RaiseSensorsStatesChangedEvent() => SensorsStatesChangedEventHandler();

        private void RaiseClickedOnMachineModeEvent() => ClickedOnMachineModeEventHandler();

        private void RaiseClickedOnMachineOnMarchEvent() => ClickedOnMachineOnMarchEventHandler();

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
            this.NavigationRegionCurrentViewModel = (MainWindowNavigationButtonsViewModel)this.Container.Resolve<IMainWindowNavigationButtonsViewModel>();
            this.ExitViewButtonRegionCurrentViewModel = null;
            this.ContentRegionCurrentViewModel = (IdleViewModel)this.Container.Resolve<IIdleViewModel>();
            this.ConnectMethod();
            this.InitializeEvents();
        }

        private async void ConnectToInstallationHubMethod()
        {
            try
            {
                this.installationClient = new InstallationHubClient("http://localhost:5000", "/installation-endpoint");
                await this.installationClient.ConnectAsync();
                this.installationClient.ReceivedMessageToAllConnectedClients += this.Client_ReceivedMessageToAllConnectedClients;
                installationClient.connection.Closed += async (error) => InternalMessages = "Not Connected";
                this.InternalMessages = "Connected!";
            }
            catch (Exception exception)
            {
                this.InternalMessages = "Not Connected!";
            }
        }

        private void Client_ReceivedMessageToAllConnectedClients(object sender, string message)
        {
            this.MessageFromServer = message;
        }

        #endregion Methods
    }
}
