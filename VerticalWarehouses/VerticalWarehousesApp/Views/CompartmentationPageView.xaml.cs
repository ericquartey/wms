using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Media;
using Ferretto.VW.CustomControls.Controls;
using System.Diagnostics;

namespace Ferretto.VW.VerticalWarehousesApp.Views
{
    public partial class CompartmentationPageView : Page
    {
        const int RESOLUTION = 20; //passo minimo di lunghezza scomparto

        Point testPoint;
        Point currentRectStartPoint;

        CompartmentRectangle currentRect;
        List<CompartmentRectangle> rects;

        CompartmentActionMode currentActionMode = CompartmentActionMode.NoActionSelectedYet;

        bool isDrawing = false;

        delegate void FocusableMethodHandler();

        public CompartmentationPageView()
        {
            this.InitializeComponent();
            this.rects = new List<CompartmentRectangle>();
            this.CheckActionButtonsSelectionCorrectness();
        }

        private void ImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.currentActionMode == CompartmentActionMode.DrawCompartment)
            {
                Debug.Print("Start pos: " + e.GetPosition(this) + "\n");
                if (this.isDrawing)
                {

                }
                else
                {
                    if ((int)e.GetPosition(this).X % RESOLUTION == 0)
                    {
                        this.testPoint.X = e.GetPosition(this).X;
                    }else
                    {
                        this.testPoint.X = (int)e.GetPosition(this).X - ((int)e.GetPosition(this).X % RESOLUTION);
                    }
                    if ((int)e.GetPosition(this).Y % RESOLUTION == 0)
                    {
                        this.testPoint.Y = e.GetPosition(this).Y;
                    }
                    else
                    {
                        this.testPoint.Y = (int)e.GetPosition(this).Y - ((int)e.GetPosition(this).Y % RESOLUTION);
                    }
                    if (this.testPoint.X < 20)
                    {
                        this.testPoint.X = 20;
                    }
                    if (this.testPoint.X > 760)
                    {
                        this.testPoint.X = 760;
                    }
                    if (this.testPoint.Y < 60)
                    {
                        this.testPoint.Y = 60;
                    }
                    if (this.testPoint.Y > 580)
                    {
                        this.testPoint.Y = 580;
                    }
                    
                    this.currentRectStartPoint = this.testPoint;
                    this.currentRect = new CompartmentRectangle();
                    this.currentRect.Height = 0;
                    this.currentRect.Width = 0;
                    this.cnvImage.Children.Add(this.currentRect);

                    this.currentRect.SetValue(Canvas.LeftProperty, this.testPoint.X);
                    this.currentRect.SetValue(Canvas.TopProperty, this.testPoint.Y - 50); //50 = correzione: il canvas è posto nella 2a riga del Grid, con la prima riga alta 50px

                    this.isDrawing = true;
                }
            }
        }

        private void ImageMouseMove(object sender, MouseEventArgs e)
        {
            if (this.currentActionMode == CompartmentActionMode.DrawCompartment)
            {
                if (this.isDrawing)
                {
                    this.testPoint = e.GetPosition(this);
                    // if I want continous lenghts for the compartment, then: double x = (this.testPoint.X - this.currentRectStartPoint.X); double y = (this.testPoint.Y - this.currentRectStartPoint.Y);
                    int x = ((int)(this.testPoint.X - this.currentRectStartPoint.X) % RESOLUTION == 0) ? (int)(this.testPoint.X - this.currentRectStartPoint.X) : ((int)(this.testPoint.X - this.currentRectStartPoint.X) - ((int)(this.testPoint.X - this.currentRectStartPoint.X) % RESOLUTION));

                    int y = ((int)(this.testPoint.Y - this.currentRectStartPoint.Y) % RESOLUTION == 0) ? (int)(this.testPoint.Y - this.currentRectStartPoint.Y) : ((int)(this.testPoint.Y - this.currentRectStartPoint.Y) - ((int)(this.testPoint.Y - this.currentRectStartPoint.Y) % RESOLUTION));

                    if (x >= 0)
                    {
                        this.currentRect.Width = (double)x;
                    }
                    if (y >= 0)
                    {
                        this.currentRect.Height = (double)y;
                    }
                    if (this.currentRectStartPoint.X + x > 760)
                    {
                        this.currentRect.Width = 760 - this.currentRectStartPoint.X;
                    }
                    if (this.currentRectStartPoint.Y + y > 580)
                    {
                        this.currentRect.Height = 580 - this.currentRectStartPoint.Y;
                    }
                }
            }
        }

        private void ImageMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.currentActionMode == CompartmentActionMode.DrawCompartment)
            {
                if (this.isDrawing)
                {
                    this.rects.Add(this.currentRect);
                    this.currentRect = null;
                    this.isDrawing = false;
                }
            }
        }

        private void ButtonSetActionToDrawCompartment(object sender, RoutedEventArgs e)
        {
            this.SetActionToDrawCompartment();
            this.CheckActionButtonsSelectionCorrectness();
        }

        private void ButtonSetActionToCreateCompartment(object sender, RoutedEventArgs e)
        {
            this.SetActionToCreateCompartment();
            this.CheckActionButtonsSelectionCorrectness();
            this.CreateCompartment(100, 100, 100, 123);
        }

        private void SetActionToDrawCompartment()
        {
            this.currentActionMode = CompartmentActionMode.DrawCompartment;
        }

        private void SetActionToCreateCompartment()
        {
            this.currentActionMode = CompartmentActionMode.CreateCompartment;
        }

        private void CreateCompartment(double originx, double originy, double width, double height)
        {
            int ox, oy, w, h;

            ox = this.NormalizeXValue(originx);
            oy = this.NormalizeYValue(originy);
            w = this.NormalizeXValue(width);
            h = this.NormalizeYValue(height);

            if (ox + w > 760)
            {
                w = 760 - ox;
            }
            if (oy + h > 580)
            {
                h = 580 - oy;
            }

            this.currentRect = new CompartmentRectangle();
            this.currentRect.SetValue(Canvas.LeftProperty, (double)ox);
            this.currentRect.SetValue(Canvas.TopProperty, (double)oy - 50);
            this.currentRect.Width = w;
            this.currentRect.Height = h;
            this.cnvImage.Children.Add(this.currentRect);
            this.rects.Add(this.currentRect);
            this.currentRect = null;
        }

        private void CheckActionButtonsSelectionCorrectness()
        {
            switch(this.currentActionMode)
            {
                case CompartmentActionMode.DrawCompartment:
                    this.ButtonDrawCompartments.Foreground = Brushes.Green;
                    foreach(Button btn in this.DrawButtonsStackPanel.Children)
                    {
                        if (!(btn.Name == "ButtonDrawCompartments"))
                        {
                            btn.Foreground = Brushes.White;
                        }
                    }
                    break;
                case CompartmentActionMode.CreateCompartment:
                    this.ButtonDoNothing.Foreground = Brushes.Green;
                    foreach (Button btn in this.DrawButtonsStackPanel.Children)
                    {
                        if (!(btn.Name == "ButtonDoNothing"))
                        {
                            btn.Foreground = Brushes.White;
                        }
                    }
                    break;
                case CompartmentActionMode.NoActionSelectedYet:
                    break;
            }
        }

        int NormalizeXValue(double x)
        {
            int ret_x;

            if (x >= 20 && x <= 760)
            {
                ret_x = ((int)x % RESOLUTION == 0) ? (int)x : (int)x - ((int)x % RESOLUTION);
            }
            else
            {
                ret_x = (x < 20) ? 20 : 760;
            }
            return ret_x;
        }
        int NormalizeYValue(double y)
        {
            int ret_y;

            if (y >= 20 && y <= 760)
            {
                ret_y = ((int)y % RESOLUTION == 0) ? (int)y : (int)y - ((int)y % RESOLUTION);
            }
            else
            {
                ret_y = (y < 20) ? 20 : 760;
            }
            return ret_y;
        }
    }

    public enum CompartmentActionMode
    {
        DrawCompartment,
        CreateCompartment,
        NoActionSelectedYet
    }
}
