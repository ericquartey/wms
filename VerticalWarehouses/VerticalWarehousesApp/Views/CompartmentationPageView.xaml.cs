using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Media;

namespace Ferretto.VW.VerticalWarehousesApp.Views
{
    public partial class CompartmentationPageView : Page
    {
        Point testPoint;
        Point currentRectStartPoint;

        Rectangle currentRect;
        List<Rectangle> rects;

        bool isDrawing = false;

        public CompartmentationPageView()
        {
            this.InitializeComponent();
            this.rects = new List<Rectangle>();
        }

        private void imgCamera_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.isDrawing)
            {

            } else
            {
                this.testPoint = e.GetPosition(this);
                this.currentRectStartPoint = this.testPoint;
                this.currentRect = new Rectangle();
                this.currentRect.Height = 0;
                this.currentRect.Width = 0;
                this.currentRect.Fill = Brushes.Red;
                this.cnvImage.Children.Add(this.currentRect);

                this.currentRect.SetValue(Canvas.LeftProperty, this.testPoint.X);
                this.currentRect.SetValue(Canvas.TopProperty, this.testPoint.Y + 50);

                this.isDrawing = true;
            }
        }

        private void imgCamera_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isDrawing)
            {
                this.testPoint = e.GetPosition(this);

                double x = this.testPoint.X - this.currentRectStartPoint.X;
                double y = this.testPoint.Y - this.currentRectStartPoint.Y;

                if (x >= 0)
                {
                    this.currentRect.Width = x;
                }
                if (y >= 0)
                {
                    this.currentRect.Height = y;
                }
            }
        }

        private void imgCamera_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.isDrawing)
            {
                this.rects.Add(this.currentRect);
                this.currentRect = null;
                this.isDrawing = false;
            }
        }
    }
}
