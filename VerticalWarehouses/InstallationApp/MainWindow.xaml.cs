using System;
using System.Collections.Generic;
using System.Windows;
using Ferretto.VW.Navigation;
using System.Windows.Input;

namespace Ferretto.VW.InstallationApp
{
    public partial class MainWindow : Window
    {
        //public MainWindow()
        //{
        //    this.InitializeComponent();
        //    NavigationService.BackToVWAppEventHandler += this.CloseThisMainWindow;
        //    this.DataContext = new MainWindowViewModel();
        //}

        #region Methods

        public void BackToVWAppButtonMethod(object sender, MouseButtonEventArgs e)
        {
            this.BackToVWApp();
        }

        public void InitializeNavigation()
        {
            NavigationService.BackToVWAppEventHandler += this.CloseThisMainWindow;
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
