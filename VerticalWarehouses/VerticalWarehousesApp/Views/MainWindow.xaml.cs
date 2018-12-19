using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.VW.VerticalWarehousesApp.ViewModels;
using Ferretto.VW.VerticalWarehousesApp.Views;

namespace Ferretto.VW.VerticalWarehousesApp
{
    public partial class MainWindow : Window
    {
        #region Fields

        private Page currentTestConnectionPage, currentCompartmentationPage;
        private string drawerSelected;
        private MainWindowViewModel mwvm;

        #endregion Fields

        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
            this.currentTestConnectionPage = new TestConnectionPageView();
            this._NavigationRegion.Navigate(this.currentTestConnectionPage);
        }

        #endregion Constructors

        #region Methods

        public void CloseUserLogInPopUp(object sender, EventArgs e)
        {
            this.SetButtonRegionActive();
            this.mwvm.IsUserLoggedIn = true;
            this.UserLoginPopup.IsOpen = false;
            this.SetUserLoggedInRectColour();
        }

        public void OpenUserLogInPopUp(object sender, EventArgs e)
        {
            this.UserLoginPopup.IsOpen = true;
        }

        private void ButtonNavigateToCompartmentationPage(object sender, RoutedEventArgs e)
        {
            this.NavigateToCompartmentationPage();
        }

        private void ButtonNavigateToTestConnectionPage(object sender, RoutedEventArgs e)
        {
            this.NavigateToTestConnectionPage();
        }

        private void NavigateToCompartmentationPage()
        {
            this._NavigationRegion.Navigate(this.currentCompartmentationPage);
        }

        private void NavigateToTestConnectionPage()
        {
            this._NavigationRegion.Navigate(this.currentTestConnectionPage);
        }

        private void SetButtonRegionActive()
        {
            for (var i = 0; i < this.ButtonRegionStackPanel.Children.Count; i++)
            {
                this.ButtonRegionStackPanel.Children[i].IsEnabled = true;
            }
        }

        private void SetUserLoggedInRectColour()
        {
            if (!this.mwvm.IsUserLoggedIn)
            {
                this.UserLoggedRect.Fill = new SolidColorBrush(Colors.Red);
            }
            else
            {
                this.UserLoggedRect.Fill = new SolidColorBrush(Colors.Green);
            }
        }

        private void VMLoaded(object sender, RoutedEventArgs e)
        {
            this.mwvm = new MainWindowViewModel();
        }

        #endregion Methods
    }
}
