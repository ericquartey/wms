using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Ferretto.VW.VerticalWarehousesApp.ViewModels;
using Ferretto.VW.VerticalWarehousesApp.Views;

namespace Ferretto.VW.VerticalWarehousesApp
{
  public partial class MainWindow : Window
  {
        MainWindowViewModel mwvm;
        Page currentTestConnectionPage, currentCompartmentationPage;

        public MainWindow()
        {            
            this.InitializeComponent();
            this.currentTestConnectionPage = new TestConnectionPageView();
            this.currentCompartmentationPage = new CompartmentationPageView();
            this._NavigationRegion.Navigate(this.currentTestConnectionPage);
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

        #region navigation
        private void NavigateToTestConnectionPage()
        {
            this._NavigationRegion.Navigate(this.currentTestConnectionPage);
        }

        private void NavigateToCompartmentationPage()
        {
            this._NavigationRegion.Navigate(this.currentCompartmentationPage);
        }

        //private void NavigateToManualPage()
        //{
        //    this._NavigationRegion.Navigate(new ManualCompartmentPage());
        //}

        public void NavigateToDrawerPage()
        {
            this._NavigationRegion.Navigate(new DrawerPage(this));
        }

        public void NavigateToManualPage()
        {
            this._NavigationRegion.Navigate(new ManualCompartmentPage(this));
        }

        public void NavigateToDrawGridPage()
        {
            this._NavigationRegion.Navigate(new DrawGridPage());
        }
        #endregion

        #region controls
        private void ButtonNavigateToTestConnectionPage(object sender, RoutedEventArgs e)
        {
            this.NavigateToTestConnectionPage();
        }

        private void ButtonNavigateToCompartmentationPage(object sender, RoutedEventArgs e)
        {
            this.NavigateToCompartmentationPage();
        }

        private void ButtonNavigateToManualPage(Object sender, RoutedEventArgs e)
        {
            this.NavigateToDrawerPage();
        }
        #endregion
    }
}
