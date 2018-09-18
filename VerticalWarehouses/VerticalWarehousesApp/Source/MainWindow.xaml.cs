using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Ferretto.VW.ActionBlocks;
using Ferretto.VW.VerticalWarehousesApp.ViewModels;

namespace Ferretto.VW.VerticalWarehousesApp
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
        MainWindowViewModel mwvm;
        public MainWindow()
        {
            this.InitializeComponent();

        CActions myActions = new CActions();
        }

        public void OpenUserLogInPopUp(object sender, EventArgs e)
         {
                this.UserLoginPopup.IsOpen = true;
          }

        public void CloseUserLogInPopUp(object sender, EventArgs e)
         {
            this.SetButtonRegionActive();
            this.mwvm.IsUserLoggedIn = true;
            this.UserLoginPopup.IsOpen = false;
            this.SetUserLoggedInRectColour();
         }

        private void VMLoaded(object sender, RoutedEventArgs e)
        {
            this.mwvm = new MainWindowViewModel();
        }

        private void SetButtonRegionActive()
        {
            for (int i = 0; i < this.ButtonRegionStackPanel.Children.Count; i++)
            {
                this.ButtonRegionStackPanel.Children[i].IsEnabled = true;
            }
        }

        private void SetUserLoggedInRectColour()
        {
            if (!this.mwvm.IsUserLoggedIn) {
                this.UserLoggedRect.Fill = new SolidColorBrush(Colors.Red);
            } else
            {
                this.UserLoggedRect.Fill = new SolidColorBrush(Colors.Green);
            }
            
        }

    }
}
