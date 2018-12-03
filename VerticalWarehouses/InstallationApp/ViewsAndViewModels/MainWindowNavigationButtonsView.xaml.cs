using Ferretto.VW.Utils.Source;
using System.Windows;
using System;
using System.Windows.Input;
using Ferretto.VW.Navigation;
using System.Diagnostics;
using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels
{
    /// <summary>
    /// Interaction logic for MainWindowNavigationButtonsView.xaml
    /// </summary>
    public partial class MainWindowNavigationButtonsView : BaseView
    {
        #region Fields

        private ScrollChangedEventArgs scrollChangedEventArgs;
        private double scrollViewerOffset;

        #endregion Fields

        #region Constructors

        public MainWindowNavigationButtonsView()
        {
            this.InitializeComponent();
            this.DataContext = new MainWindowNavigationButtonsViewModel();
            this.NavigationButtonScrollViewer.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(this.CheckVerticalOffset), true);
        }

        #endregion Constructors

        #region Methods

        private void CheckVerticalOffset(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset == 0)
            {
                this.UpScroll.IsEnabled = false;
            }
            else
            {
                this.UpScroll.IsEnabled = true;
                Debug.Print("Scroll pos != 0: " + this.NavigationButtonScrollViewer.VerticalOffset);
            }
            if (e.VerticalOffset == 322)
            {
                this.DownScroll.IsEnabled = false;
            }
            else
            {
                this.DownScroll.IsEnabled = true;
                Debug.Print("Scroll pos != 0: " + this.NavigationButtonScrollViewer.VerticalOffset);
            }
        }

        private void DownScroll_Click(Object sender, RoutedEventArgs e)
        {
            this.NavigationButtonScrollViewer.LineDown();
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void UpScroll_Click(Object sender, RoutedEventArgs e)
        {
            this.NavigationButtonScrollViewer.LineUp();
        }

        #endregion Methods
    }
}
