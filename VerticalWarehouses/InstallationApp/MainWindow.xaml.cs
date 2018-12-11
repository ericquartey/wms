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
        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
            NavigationService.BackToVWAppEventHandler += () => this.Close();
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

        #endregion Methods
    }
}
