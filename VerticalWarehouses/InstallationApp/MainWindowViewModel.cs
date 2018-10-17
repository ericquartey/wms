using System;
using Ferretto.VW.InstallationApp.Views;
using Prism.Mvvm;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;
using System.Windows.Media;
using System.Diagnostics;

namespace Ferretto.VW.InstallationApp
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private const string FERRETTOBLACK = "#0A0A0A";
        private const string FERRETTODARKGRAY = "#293133";
        private const string FERRETTOGREEN = "#57A639";
        private const string FERRETTOLIGHTGRAY = "#c5c7c4";
        private const string FERRETTOMEDIUMGRAY = "#707173";
        private const string FERRETTOPUREWHITE = "#ffffff";
        private const string FERRETTORED = "#e2001a";
        private const string FERRETTOWHITEGRAY = "#e6e6e6";
        private readonly LowSpeedMovementsTestView lowSpeddMovementsTestViewInstance;
        private readonly VerifyCircuitIntegrityView verifyCircuitIntegrityViewInstance;
        private readonly VerticalAxisCalibrationView verticalAxisCalibrationViewInstance;
        private UserControl currentPage;
        private bool enableComboBox1IsDropDownOpen;
        private bool enableComboBox2IsDropDownOpen;
        private bool enableComboBox3IsDropDownOpen;
        private bool enableLowSpeedMovementsTestButton;
        private bool enableVerifyCircuitIntegrityButton;
        private bool enableVerticalAxisCalibrationButton;
        private ICommand lowSpeedMovementsTestButtonCommand;
        private SolidColorBrush machineModeComboBoxForegroundBrush = new SolidColorBrush();
        private int machineModeComboBoxSelection;
        private SolidColorBrush machineOnMarchComboBoxForegroundBrush = new SolidColorBrush();
        private int machineOnMarchComboBoxSelection;
        private ICommand verifyCircuitIntegrityButtonCommand;
        private ICommand verticalAxisCalibrationButtonCommand;

        #endregion Fields

        #region Constructors

        public MainWindowViewModel()
        {
            this.lowSpeddMovementsTestViewInstance = new LowSpeedMovementsTestView();
            this.verifyCircuitIntegrityViewInstance = new VerifyCircuitIntegrityView();
            this.verticalAxisCalibrationViewInstance = new VerticalAxisCalibrationView();
        }

        #endregion Constructors

        #region Properties

        public UserControl CurrentPage { get => this.currentPage; set => this.SetProperty(ref this.currentPage, value); }
        public Boolean EnableComboBox1IsDropDownOpen { get => this.enableComboBox1IsDropDownOpen; set => this.SetProperty(ref this.enableComboBox1IsDropDownOpen, value); }
        public Boolean EnableComboBox2IsDropDownOpen { get => this.enableComboBox2IsDropDownOpen; set => this.SetProperty(ref this.enableComboBox2IsDropDownOpen, value); }
        public Boolean EnableComboBox3IsDropDownOpen { get => this.enableComboBox3IsDropDownOpen; set => this.SetProperty(ref this.enableComboBox3IsDropDownOpen, value); }
        public Boolean EnableLowSpeedMovementsTestButton { get => this.enableLowSpeedMovementsTestButton; set => this.enableLowSpeedMovementsTestButton = value; }
        public Boolean EnableVerifyCircuitIntegrityButton { get => this.enableVerifyCircuitIntegrityButton; set => this.enableVerifyCircuitIntegrityButton = value; }
        public Boolean EnableVerticalAxisCalibrationButton { get => this.enableVerticalAxisCalibrationButton; set => this.enableVerticalAxisCalibrationButton = value; }
        public ICommand LowSpeedMovementsTestButtonCommand => this.lowSpeedMovementsTestButtonCommand ?? (this.lowSpeedMovementsTestButtonCommand = new DelegateCommand(this.ExecuteLowSpeedMovementsTestButtonCommand));

        public SolidColorBrush MachineModeComboBoxForegroundBrush
        {
            get => this.machineModeComboBoxForegroundBrush; set
            {
                Debug.Print("Selected machineMode: " + this.MachineModeComboBoxSelection + "\n");
                switch (this.MachineModeComboBoxSelection)
                {
                    case 0:
                        this.machineModeComboBoxForegroundBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0000ff"));
                        this.SetProperty(ref this.machineModeComboBoxForegroundBrush, (SolidColorBrush)(new BrushConverter().ConvertFrom("#0000ff")));
                        break;

                    case 1:
                        this.machineModeComboBoxForegroundBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom(FERRETTOGREEN));
                        this.SetProperty(ref this.machineModeComboBoxForegroundBrush, (SolidColorBrush)(new BrushConverter().ConvertFrom(FERRETTOGREEN)));
                        break;

                    default:
                        this.SetProperty(ref this.machineModeComboBoxForegroundBrush, (SolidColorBrush)(new BrushConverter().ConvertFrom(FERRETTOPUREWHITE)));
                        break;
                }
            }
        }

        public Int32 MachineModeComboBoxSelection { get => this.machineModeComboBoxSelection; set => this.SetProperty(ref this.machineModeComboBoxSelection, value); }

        public SolidColorBrush MachineOnMarchComboBoxForegroundBrush
        {
            get => this.machineOnMarchComboBoxForegroundBrush;
            set
            {
                switch (this.MachineOnMarchComboBoxSelection)
                {
                    case 0:
                        this.machineOnMarchComboBoxForegroundBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom(FERRETTOGREEN));
                        this.SetProperty(ref this.machineOnMarchComboBoxForegroundBrush, (SolidColorBrush)(new BrushConverter().ConvertFrom(FERRETTOGREEN)));
                        break;

                    case 1:
                        this.machineOnMarchComboBoxForegroundBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom(FERRETTOWHITEGRAY));
                        this.SetProperty(ref this.machineOnMarchComboBoxForegroundBrush, (SolidColorBrush)(new BrushConverter().ConvertFrom(FERRETTOWHITEGRAY)));
                        break;

                    default:
                        this.SetProperty(ref this.machineOnMarchComboBoxForegroundBrush, (SolidColorBrush)(new BrushConverter().ConvertFrom(FERRETTOPUREWHITE)));
                        break;
                }
            }
        }

        public Int32 MachineOnMarchComboBoxSelection { get => this.machineOnMarchComboBoxSelection; set => this.SetProperty(ref this.machineOnMarchComboBoxSelection, value); }
        public ICommand VerifyCircuitIntegrityButtonCommand => this.verifyCircuitIntegrityButtonCommand ?? (this.verifyCircuitIntegrityButtonCommand = new DelegateCommand(this.ExecuteVerifyCircuitIntegrityButtonCommand));
        public ICommand VerticalAxisCalibrationButtonCommand => this.verticalAxisCalibrationButtonCommand ?? (this.verticalAxisCalibrationButtonCommand = new DelegateCommand(this.ExecuteVerticalAxisCalibrationButtonCommand));

        #endregion Properties

        #region Methods

        private void ExecuteLowSpeedMovementsTestButtonCommand()
        {
            this.CurrentPage = this.lowSpeddMovementsTestViewInstance;
        }

        private void ExecuteVerifyCircuitIntegrityButtonCommand()
        {
            this.CurrentPage = this.verifyCircuitIntegrityViewInstance;
        }

        private void ExecuteVerticalAxisCalibrationButtonCommand()
        {
            this.CurrentPage = this.verticalAxisCalibrationViewInstance;
        }

        #endregion Methods
    }
}
