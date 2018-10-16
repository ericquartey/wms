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
            this.DataContext = new MainWindowViewModel();
        }

        #endregion Constructors

        #region Methods

        public void BackToVWAppButtonMethod(object sender, MouseButtonEventArgs e)
        {
            this.BackToVWApp();
        }

        public void ChangeComboBox1IsDropDownOpenButtonMethod(object sender, MouseButtonEventArgs e)
        {
            this.ChangeComboBox1IsDropDownOpen();
        }

        public void ChangeComboBox2IsDropDownOpenButtonMethod(object sender, MouseButtonEventArgs e)
        {
            this.ChangeComboBox2IsDropDownOpen();
        }

        public void ChangeComboBox3IsDropDownOpenButtonMethod(object sender, MouseButtonEventArgs e)
        {
            this.ChangeComboBox3IsDropDownOpen();
        }

        private void BackToVWApp()
        {
            NavigationService.RaiseBackToVWAppEvent();
        }

        private void ChangeComboBox1IsDropDownOpen()
        {
            var d = this.DataContext as MainWindowViewModel;
            if (d != null)
            {
                if (d.EnableComboBox1IsDropDownOpen)
                {
                    Action action = () => d.EnableComboBox1IsDropDownOpen = false;
                    Application.Current.Dispatcher.BeginInvoke(action);
                }
                else
                {
                    Action action = () => d.EnableComboBox1IsDropDownOpen = true;
                    Application.Current.Dispatcher.BeginInvoke(action);
                }
            }
        }

        private void ChangeComboBox2IsDropDownOpen()
        {
            var d = this.DataContext as MainWindowViewModel;
            if (d != null)
            {
                d.EnableComboBox2IsDropDownOpen = !d.EnableComboBox2IsDropDownOpen;
            }
        }

        private void ChangeComboBox3IsDropDownOpen()
        {
            MainWindowViewModel d = (MainWindowViewModel)this.DataContext;
            if (d != null)
            {
                d.EnableComboBox3IsDropDownOpen = !d.EnableComboBox3IsDropDownOpen;
            }
        }

        private void CloseThisMainWindow()
        {
            this.Close();
        }

        #endregion Methods
    }
}
