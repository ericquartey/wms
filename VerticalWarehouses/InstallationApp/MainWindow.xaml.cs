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
            NavigationService.BackToVWAppEventHandler += this.CloseThisMainWindow;
            this.InitializeComponent();
        }

        #endregion Constructors

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
