using Ferretto.VW.Utils.Source;
using System.Windows;
using System;
using System.Windows.Input;
using Ferretto.VW.Navigation;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels
{
    /// <summary>
    /// Interaction logic for MainWindowNavigationButtonsView.xaml
    /// </summary>
    public partial class MainWindowNavigationButtonsView : BaseView
    {
        #region Fields

        private double scrollViewerOffset;

        #endregion Fields

        #region Constructors

        public MainWindowNavigationButtonsView()
        {
            this.InitializeComponent();
            this.DataContext = new MainWindowNavigationButtonsViewModel();
            NavigationService.SaveScrollViewerOffsetEventHandler += this.SaveScrollViewerOffset;
            NavigationService.LoadScrollViewerOffsetEventHandler += this.LoadScrollViewerOffset;
        }

        #endregion Constructors

        #region Methods

        private void LoadScrollViewerOffset()
        {
            this.NavigationButtonScrollViewer.ScrollToVerticalOffset(this.scrollViewerOffset);
        }

        private void SaveScrollViewerOffset()
        {
            this.scrollViewerOffset = this.NavigationButtonScrollViewer.VerticalOffset;
        }

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
