using System.Windows.Input;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.Resources;
using Ferretto.VW.OperatorApp.Resources.Enumerations;
using Ferretto.VW.OperatorApp.ViewsAndViewModels;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Interfaces;
using Ferretto.VW.Utils.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp
{
    public delegate void ClickedOnMachineModeEvent();

    public delegate void ClickedOnMachineOnMarchEvent();

    public partial class MainWindowViewModel : BindableBase, IMainWindowViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly HelpMainWindow helpWindow;

        private IUnityContainer container;

        private BindableBase contentRegionCurrentViewModel;

        private BindableBase exitViewButtonRegionCurrentViewModel;

        private bool isPopupOpen;

        private bool machineModeSelectionBool;

        private bool machineOnMarchSelectionBool;

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

        public bool IsPopupOpen { get => this.isPopupOpen; set => this.SetProperty(ref this.isPopupOpen, value); }

        public bool MachineModeSelectionBool { get => this.machineModeSelectionBool; set => this.SetProperty(ref this.machineModeSelectionBool, value); }

        public bool MachineOnMarchSelectionBool { get => this.machineOnMarchSelectionBool; set => this.SetProperty(ref this.machineOnMarchSelectionBool, value); }

        public BindableBase NavigationRegionCurrentViewModel { get => this.navigationRegionCurrentViewModel; set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value); }

        public ICommand OpenClosePopupCommand => this.openClosePopupCommand ?? (this.openClosePopupCommand = new DelegateCommand(() => this.IsPopupOpen = !this.IsPopupOpen));

        #endregion

        #region Methods

        public void ChangeDrawerOperationView()
        {
            this.contentRegionCurrentViewModel = this.container.Resolve<IDrawerActivityPickingViewModel>() as DrawerActivityPickingViewModel;
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.NavigationRegionCurrentViewModel = this.container.Resolve<IMainWindowNavigationButtonsViewModel>() as MainWindowNavigationButtonsViewModel;
            this.ExitViewButtonRegionCurrentViewModel = null;
            this.ContentRegionCurrentViewModel = (IdleViewModel)this.container.Resolve<IIdleViewModel>();
            this.InitializeEvents();

            this.eventAggregator.GetEvent<NotificationEventUI<DrawerOperationMessageData>>().Subscribe(
                message => this.ChangeDrawerOperationView());
        }

        private static void RaiseClickedOnMachineModeEvent() => ClickedOnMachineModeEventHandler();

        private static void RaiseClickedOnMachineOnMarchEvent() => ClickedOnMachineOnMarchEventHandler();

        private void InitializeEvents()
        {
            MainWindow.FinishedMachineModeChangeStateEventHandler += () => { this.MachineModeSelectionBool = !this.MachineModeSelectionBool; };
            MainWindow.FinishedMachineOnMarchChangeStateEventHandler += () => { this.MachineOnMarchSelectionBool = !this.MachineOnMarchSelectionBool; };
            ClickedOnMachineModeEventHandler += () => { };
            ClickedOnMachineOnMarchEventHandler += () => { };
        }

        #endregion
    }
}
