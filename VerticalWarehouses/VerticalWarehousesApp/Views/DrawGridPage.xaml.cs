using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ferretto.VW.VerticalWarehousesApp.Views
{
    /// <summary>
    /// Interaction logic for DrawGrid.xaml
    /// </summary>
    public partial class DrawGridPage : Page
    {
        private const int xOrigin = 15;
        private const int yOrigin = 15;
        private const int square = 25;
        private const double offset_writing = 7.5f;

        public DrawGridPage()
        {
            this.InitializeComponent();
            Rectangle rect = new Rectangle();
            //rect.PointToScreen(new Point(0,0));
            Canvas.SetLeft(rect, xOrigin);
            Canvas.SetTop(rect, yOrigin);
            rect.Width = 50;
            rect.Height = 50;
            rect.Fill = new SolidColorBrush(Colors.Red);
            rect.Opacity = 0.2f;
            this.canvas.Children.Add(rect);

            for (int i = 0; i < 10; i++)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = $"{square * i}";
                textBlock.Foreground = new SolidColorBrush(Colors.Blue);
                Canvas.SetLeft(textBlock, ((square * i) + offset_writing));
                Canvas.SetTop(textBlock, 0);
                this.canvas.Children.Add(textBlock);
            }
        }
    }
}
