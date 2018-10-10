using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.Navigation;
using Ferretto.VW.InstallationApp.Views;

namespace Ferretto.VW.InstallationApp
{
    public partial class MainWindow : Window
    {
        #region Fields

        private LowSpeedMovementsTestView lowSpeddMovementsTestViewInstance;
        private VerifyCircuitIntegrityView verifyCircuitIntegrityViewInstance;
        private VerticalAxisCalibrationView verticalAxisCalibrationViewInstance;

        #endregion Fields

        #region Constructors

        public MainWindow()
        {
            this.InstallerQualification = "Installer";
            NavigationService.BackToVWAppEventHandler += this.CloseThisMainWindow;
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public string InstallerQualification { get; set; } = "Installer";

        #endregion Properties

        #region Methods

        public void BackToVWAppButtonMethod(object sender, RoutedEventArgs e)
        {
            this.BackToVWApp();
        }

        public void OpenLowSpeedMovementsTestViewButtonMethod(object sender, RoutedEventArgs e)
        {
            this.OpenLowSpeedMovementsViewMethod();
        }

        public void OpenVerifyCircuitIntegrityViewButtonMethod(object sender, RoutedEventArgs e)
        {
            this.OpenVerifyCircuitIntegrityViewMethod();
        }

        public void OpenVerticalAxisCalibrationViewButtonMethod(object sender, RoutedEventArgs e)
        {
            this.OpenVerticalAxisCalibrationViewMethod();
        }

        private void BackToVWApp()
        {
            NavigationService.RaiseBackToVWAppEvent();
        }

        private void CloseThisMainWindow()
        {
            this.Close();
        }

        private void OpenLowSpeedMovementsViewMethod()
        {
            this.lowSpeddMovementsTestViewInstance = new LowSpeedMovementsTestView();
            this.lowSpeddMovementsTestViewInstance.Width = this.InstallationPageRegionContentControl.ActualWidth;
            this.lowSpeddMovementsTestViewInstance.Height = this.InstallationPageRegionContentControl.ActualHeight;
            this.InstallationPageRegionContentControl.Content = null;
            this.InstallationPageRegionContentControl.Content = this.lowSpeddMovementsTestViewInstance;
        }

        private void OpenVerifyCircuitIntegrityViewMethod()
        {
            this.verifyCircuitIntegrityViewInstance = new VerifyCircuitIntegrityView();
            this.verifyCircuitIntegrityViewInstance.Width = this.InstallationPageRegionContentControl.ActualWidth;
            this.verifyCircuitIntegrityViewInstance.Height = this.InstallationPageRegionContentControl.ActualHeight;
            this.InstallationPageRegionContentControl.Content = null;
            this.InstallationPageRegionContentControl.Content = this.verifyCircuitIntegrityViewInstance;
        }

        private void OpenVerticalAxisCalibrationViewMethod()
        {
            this.verticalAxisCalibrationViewInstance = new VerticalAxisCalibrationView();
            this.verticalAxisCalibrationViewInstance.Width = this.InstallationPageRegionContentControl.ActualWidth;
            this.verticalAxisCalibrationViewInstance.Height = this.InstallationPageRegionContentControl.ActualHeight;
            this.InstallationPageRegionContentControl.Content = null;
            this.InstallationPageRegionContentControl.Content = this.verticalAxisCalibrationViewInstance;
        }

        #endregion Methods
    }
}
