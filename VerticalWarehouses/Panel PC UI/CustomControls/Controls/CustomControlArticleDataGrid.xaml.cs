using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ferretto.VW.CustomControls.Controls
{
    /// <summary>
    /// Interaction logic for CustomControlArticleDataGrid.xaml
    /// </summary>
    public partial class CustomControlArticleDataGrid : UserControl
    {
        #region Fields

        private ScrollViewer scrollViewer;

        #endregion

        #region Constructors

        public CustomControlArticleDataGrid()
        {
            this.InitializeComponent();
            this.DataGrid.Loaded += this.FindScrollViewer;
        }

        #endregion

        #region Methods

        private void BottomButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.scrollViewer == null)
            {
                this.scrollViewer = this.GetScrollViewer(this.DataGrid);
            }
            this.scrollViewer?.ScrollToBottom();
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.scrollViewer == null)
            {
                this.scrollViewer = this.GetScrollViewer(this.DataGrid);
            }
            this.scrollViewer?.LineDown();
        }

        private void FindScrollViewer(object sender, RoutedEventArgs e)
        {
            this.GetScrollViewer(this.DataGrid);
        }

        private ScrollViewer GetScrollViewer(UIElement element)
        {
            if (element == null)
            {
                return null;
            }

            ScrollViewer retour = null;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element) && retour == null; i++)
            {
                if (VisualTreeHelper.GetChild(element, i) is ScrollViewer)
                {
                    retour = (ScrollViewer)VisualTreeHelper.GetChild(element, i);
                }
                else
                {
                    retour = this.GetScrollViewer(VisualTreeHelper.GetChild(element, i) as UIElement);
                }
            }
            return retour;
        }

        private void TopButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.scrollViewer == null)
            {
                this.scrollViewer = this.GetScrollViewer(this.DataGrid);
            }
            this.scrollViewer?.ScrollToTop();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.scrollViewer == null)
            {
                this.scrollViewer = this.GetScrollViewer(this.DataGrid);
            }
            this.scrollViewer?.LineUp();
        }

        #endregion
    }
}
