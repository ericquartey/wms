using System.Windows.Controls;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels
{
    /// <summary>
    /// Interaction logic for MainWindowNavigationButtonView.xaml
    /// </summary>
    public partial class MainWindowNavigationButtonsView : UserControl
    {
        #region Constructors

        public MainWindowNavigationButtonsView()
        {
            this.InitializeComponent();
            /// this.NavigationButtonScrollViewer.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(this.CheckVerticalOffset), true);
        }

        #endregion

       #region Methods

        //private void CheckVerticalOffset(object sender, ScrollChangedEventArgs e)
        //{
        //    if (e.VerticalOffset == 0)
        //    {
        //        this.UpScroll.IsEnabled = false;
        //    }
        //    else
        //    {
        //        this.UpScroll.IsEnabled = true;
        //    }
        //    if (e.VerticalOffset == 322)
        //    {
        //        this.DownScroll.IsEnabled = false;
        //    }
        //    else
        //    {
        //        this.DownScroll.IsEnabled = true;
        //    }
        //}

        //private void DownScroll_Click(object sender, RoutedEventArgs e)
        //{
        //    this.NavigationButtonScrollViewer.LineDown();
        //}

        //private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        //{
        //    e.Handled = true;
        //}

        //private void UpScroll_Click(object sender, RoutedEventArgs e)
        //{
        //    this.NavigationButtonScrollViewer.LineUp();
        //    //TODO: scroll up and down nav button height
        //}
        #endregion
    }
}
