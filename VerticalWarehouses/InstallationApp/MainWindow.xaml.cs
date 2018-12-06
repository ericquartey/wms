using System.Windows;
using Ferretto.VW.Navigation;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System;

namespace Ferretto.VW.InstallationApp
{
    public partial class MainWindow : Window
    {
        #region Fields

        private double navigationButtonViewTargetWidth = 210;

        #endregion Fields

        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
            NavigationService.BackToVWAppEventHandler += () => this.Close();
            NavigationService.ExitViewEventHandler += this.ShowPanelAnimation;
            NavigationService.GoToViewEventHandler += this.HidePanelAnimation;
            this.DataContext = new MainWindowViewModel();
        }

        #endregion Constructors

        #region Methods

        public void BackToVWAppButtonMethod(object sender, MouseButtonEventArgs e)
        {
            NavigationService.RaiseBackToVWAppEvent();
        }

        public void ChangeSkinToDark(object sender, MouseButtonEventArgs e)
        {
            NavigationService.RaiseChangeSkinToDarkEvent();
        }

        public void ChangeSkinToLight(object sender, MouseButtonEventArgs e)
        {
            NavigationService.RaiseChangeSkinToLightEvent();
        }

        public void ChangeSkinToMedium(object sender, MouseButtonEventArgs e)
        {
            NavigationService.RaiseChangeSkinToMediumEvent();
        }

        private void HidePanelAnimation()
        {
            var doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = this.InstallationPageNavigationButtonsRegionContentControl.ActualWidth;
            doubleAnimation.To = 0;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            this.InstallationPageNavigationButtonsRegionContentControl.BeginAnimation(ContentPresenter.WidthProperty, doubleAnimation);
        }

        private void ShowPanelAnimation()
        {
            var doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0;
            doubleAnimation.To = this.navigationButtonViewTargetWidth;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            this.InstallationPageNavigationButtonsRegionContentControl.BeginAnimation(ContentPresenter.WidthProperty, doubleAnimation);
        }

        #endregion Methods
    }
}
