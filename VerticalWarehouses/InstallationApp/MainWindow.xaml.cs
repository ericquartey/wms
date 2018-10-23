using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.Navigation;
using Ferretto.VW.InstallationApp.Views;
using System.Windows.Input;

namespace Ferretto.VW.InstallationApp
{
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            NavigationService.BackToVWAppEventHandler += this.CloseThisMainWindow;
            this.InitializeComponent();
            //this.DataContext = new MainWindowViewModel();
        }

        #endregion Constructors

        #region Methods

        public void BackToVWAppButtonMethod(object sender, MouseButtonEventArgs e)
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
