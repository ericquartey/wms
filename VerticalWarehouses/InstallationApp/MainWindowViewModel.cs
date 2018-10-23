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
    public class MainWindowViewModel : BindableBase
    {
        #region Fields

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
