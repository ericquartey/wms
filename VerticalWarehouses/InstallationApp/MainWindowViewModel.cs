using System;
using Ferretto.VW.InstallationApp.Views;
using Prism.Mvvm;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;
using System.Windows.Media;

namespace Ferretto.VW.InstallationApp
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly LSMTGateEngineView lsmtGateEngineViewInstance = new LSMTGateEngineView();
        private readonly LSMTHorizontalEngineView lsmtHorizontalEngineViewInstance = new LSMTHorizontalEngineView();
        private readonly LSMTNavigationButtonsView lsmtNavigationButtonsViewInstance = new LSMTNavigationButtonsView();
        private readonly LSMTVerticalEngineView lsmtVerticalEngineViewInstance = new LSMTVerticalEngineView();
        private readonly MainWindowNavigationButtonsView mainWindowNavigationButtonsViewInstance = new MainWindowNavigationButtonsView();
        private readonly SensorsStateNavigationButtonsView sensorsStateNavigationButtonsViewInstance = new SensorsStateNavigationButtonsView();
        private readonly SSVerticalAxisView ssVerticalAxisViewInstance = new SSVerticalAxisView();
        private readonly VerticalAxisCalibrationView verticalAxisCalibrationViewInstance = new VerticalAxisCalibrationView();

        private ICommand backToMainWindowNavigationButtonsViewCommand;
        private UserControl currentNavigationButtonsView;
        private UserControl currentPage;
        private bool enableLowSpeedMovementsTestButton;
        private bool enableVerifyCircuitIntegrityButton;
        private bool enableVerticalAxisCalibrationButton;
        private ICommand lowSpeedMovementsTestButtonCommand;
        private ICommand lsmtGateEngineButtonCommand;
        private ICommand lsmtHorizontalEngineButtonCommand;
        private ICommand lsmtVerticalEngineButtonCommand;
        private SolidColorBrush machineModeCircleFill = new SolidColorBrush(Colors.Blue);
        private int machineModeSelectionItem = 0;
        private SolidColorBrush machineOnMarchCircleFill = (SolidColorBrush)new BrushConverter().ConvertFrom("#c5c7c4");

        //FerrettoLightGray
        private int machineOnMarchSelectionItem = 0;

        private ICommand sensorsStateNavigationButtonsButtonCommand;
        private ICommand ssVerticalAxisButtonCommand;
        private ICommand verticalAxisCalibrationButtonCommand;

        #endregion Fields

        #region Constructors

        public MainWindowViewModel()
        {
            this.mainWindowNavigationButtonsViewInstance.DataContext = this;
            this.lsmtNavigationButtonsViewInstance.DataContext = this;
            this.sensorsStateNavigationButtonsViewInstance.DataContext = this;
            this.CurrentNavigationButtonsView = this.mainWindowNavigationButtonsViewInstance;
        }

        #endregion Constructors

        #region Properties

        public ICommand BackToMainWindowNavigationButtonsViewCommand => this.backToMainWindowNavigationButtonsViewCommand ?? (this.backToMainWindowNavigationButtonsViewCommand = new DelegateCommand(() => { this.CurrentNavigationButtonsView = this.mainWindowNavigationButtonsViewInstance; this.CurrentPage = null; }));
        public UserControl CurrentNavigationButtonsView { get => this.currentNavigationButtonsView; set => this.SetProperty(ref this.currentNavigationButtonsView, value); }
        public UserControl CurrentPage { get => this.currentPage; set => this.SetProperty(ref this.currentPage, value); }
        public Boolean EnableLowSpeedMovementsTestButton { get => this.enableLowSpeedMovementsTestButton; set => this.enableLowSpeedMovementsTestButton = value; }
        public Boolean EnableVerifyCircuitIntegrityButton { get => this.enableVerifyCircuitIntegrityButton; set => this.enableVerifyCircuitIntegrityButton = value; }
        public Boolean EnableVerticalAxisCalibrationButton { get => this.enableVerticalAxisCalibrationButton; set => this.enableVerticalAxisCalibrationButton = value; }
        public ICommand LowSpeedMovementsTestButtonCommand => this.lowSpeedMovementsTestButtonCommand ?? (this.lowSpeedMovementsTestButtonCommand = new DelegateCommand(() => { this.CurrentNavigationButtonsView = this.lsmtNavigationButtonsViewInstance; this.CurrentPage = null; }));
        public ICommand LSMTGateEngineButtonCommand => this.lsmtGateEngineButtonCommand ?? (this.lsmtGateEngineButtonCommand = new DelegateCommand(() => { this.CurrentPage = this.lsmtGateEngineViewInstance; }));
        public ICommand LSMTHorizontalEngineButtonCommand => this.lsmtHorizontalEngineButtonCommand ?? (this.lsmtHorizontalEngineButtonCommand = new DelegateCommand(() => { this.CurrentPage = this.lsmtHorizontalEngineViewInstance; }));
        public ICommand LSMTVerticalEngineButtonCommand => this.lsmtVerticalEngineButtonCommand ?? (this.lsmtVerticalEngineButtonCommand = new DelegateCommand(() => { this.CurrentPage = this.lsmtVerticalEngineViewInstance; }));
        public SolidColorBrush MachineModeCircleFill { get => this.machineModeCircleFill; set => this.SetProperty(ref this.machineModeCircleFill, value); }
        public SolidColorBrush[] MachineModeCircleFillArray { get; set; } = new SolidColorBrush[] { new SolidColorBrush(Colors.Blue), (SolidColorBrush)new BrushConverter().ConvertFrom("#57A639") /*FerrettoGreen*/};
        public Int32 MachineModeSelectionItem { get => this.machineModeSelectionItem; set { this.SetProperty(ref this.machineModeSelectionItem, value); this.MachineModeCircleFill = this.MachineModeCircleFillArray[value]; } }
        public SolidColorBrush MachineOnMarchCircleFill { get => this.machineOnMarchCircleFill; set => this.SetProperty(ref this.machineOnMarchCircleFill, value); }
        public SolidColorBrush[] MachineOnMarchCircleFillArray { get; set; } = new SolidColorBrush[] { (SolidColorBrush)new BrushConverter().ConvertFrom("#c5c7c4")/*FerrettoLightGray*/, (SolidColorBrush)new BrushConverter().ConvertFrom("#57A639")/*FerrettoGreen*/ };
        public Int32 MachineOnMarchSelectionItem { get => this.machineOnMarchSelectionItem; set { this.SetProperty(ref this.machineOnMarchSelectionItem, value); this.MachineOnMarchCircleFill = this.MachineOnMarchCircleFillArray[value]; } }
        public ICommand SensorsStateNavigationButtonsButtonCommand => this.sensorsStateNavigationButtonsButtonCommand ?? (this.sensorsStateNavigationButtonsButtonCommand = new DelegateCommand(() => { this.CurrentNavigationButtonsView = this.sensorsStateNavigationButtonsViewInstance; this.CurrentPage = null; }));
        public ICommand SsVerticalAxisButtonCommand => this.ssVerticalAxisButtonCommand ?? (this.ssVerticalAxisButtonCommand = new DelegateCommand(() => { this.CurrentPage = this.ssVerticalAxisViewInstance; }));
        public ICommand VerticalAxisCalibrationButtonCommand => this.verticalAxisCalibrationButtonCommand ?? (this.verticalAxisCalibrationButtonCommand = new DelegateCommand(() => { this.CurrentPage = this.verticalAxisCalibrationViewInstance; }));

        #endregion Properties
    }
}
