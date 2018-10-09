using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.Navigation;

namespace Ferretto.VW.InstallationApp
{
    public partial class MainWindow : Window
    {
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

        private void BackToVWApp()
        {
            NavigationService.RaiseBackToVWAppEvent();
        }

        private void CloseThisMainWindow()
        {
            this.Close();
        }

        #endregion Methods
    }
}
