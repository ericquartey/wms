using Ferretto.VW.Utils.Source;
using System.Windows;
using System;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Ferretto.VW.InstallationApp
{
    /// <summary>
    /// Interaction logic for MainWindowNavigationButtonsView.xaml
    /// </summary>
    public partial class MainWindowNavigationButtonsView : UserControl
    {
        #region Constructors

        public MainWindowNavigationButtonsView()
        {
            this.InitializeComponent();
            this.NavigationButtonScrollViewer.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(this.CheckVerticalOffset), true);
        }

        #endregion

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
            }
            if (e.VerticalOffset == 322)
            {
                this.DownScroll.IsEnabled = false;
            }
            else
            {
                this.DownScroll.IsEnabled = true;
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
            //TODO: scroll up and down nav button height
        }

        #endregion
    }
}
