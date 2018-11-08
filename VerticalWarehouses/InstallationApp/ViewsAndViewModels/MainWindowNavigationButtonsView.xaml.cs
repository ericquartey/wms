using Ferretto.VW.Utils.Source;
using System.Windows;
using System;
using System.Windows.Input;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels
{
    /// <summary>
    /// Interaction logic for MainWindowNavigationButtonsView.xaml
    /// </summary>
    public partial class MainWindowNavigationButtonsView : BaseView
    {
        #region Constructors

        public MainWindowNavigationButtonsView()
        {
            this.InitializeComponent();
            this.DataContext = new MainWindowNavigationButtonsViewModel();
        }

        #endregion Constructors

        #region Methods

        private void ScrollDownButtonClick(Object sender, RoutedEventArgs e)
        {
            this.NavigationButtonScrollViewer.LineDown();
        }

        private void ScrollUpButtonClick(Object sender, RoutedEventArgs e)
        {
            this.NavigationButtonScrollViewer.LineUp();
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        #endregion Methods
    }
}
