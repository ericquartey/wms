using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Media;
using Ferretto.VW.CustomControls.Controls;
using System.Diagnostics;

namespace Ferretto.VW.VerticalWarehousesApp.Views
{
    public partial class CompartmentationPageView : Page
    {
        const int RESOLUTION = 20; //smaller drawing step
        const int MAX_WIDTH = 760, MIN_WIDTH = 20; //min & max width
        const int MAX_HEIGHT = 580, MIN_HEIGHT = 60; //min & max height

        public static CompartmentRectangle selectedRect;

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
                    this.testPoint.X = this.NormalizeXValue(e.GetPosition(this).X);
                    this.testPoint.Y = this.NormalizeYValue(e.GetPosition(this).Y);
                    
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
                    if (this.currentRectStartPoint.X + x > MAX_WIDTH)
                    {
                        this.currentRect.Width = MAX_WIDTH - this.currentRectStartPoint.X;
                    }
                    if (this.currentRectStartPoint.Y + y > MAX_HEIGHT)
                    {
                        this.currentRect.Height = MAX_HEIGHT - this.currentRectStartPoint.Y;
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
                    this.currentRect.OriginX = this.currentRectStartPoint.X;
                    this.currentRect.OriginY = this.currentRectStartPoint.Y;
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
            this.CreateCompartment(100, 100, 100, 120);
        }

        private void ButtonDoDeleteCompartment(object sender, RoutedEventArgs e)
        {
            this.DeleteCompartment();
        }

        private void ButtonDoMassiveDivideCompartment(object sender, RoutedEventArgs e)
        {
            this.MassiveDivideCompartment(5,5);
        }

        private void DeleteCompartment()
        {
            var selectedRect = Keyboard.FocusedElement;

            if (selectedRect != null)
            {
                if (selectedRect.GetType() == typeof(CompartmentRectangle))
                {
                    this.rects.Remove((CompartmentRectangle)selectedRect);
                    this.cnvImage.Children.Remove((CompartmentRectangle)selectedRect);
                }
            }          
        }

        private void MassiveDivideCompartment(int rows, int columns)
        {
            var selectedRect = Keyboard.FocusedElement;
            double cellWidth, cellHeight, originX, originY;
            if (selectedRect != null)
            {
                if (selectedRect.GetType() == typeof(CompartmentRectangle))
                {
                    cellWidth = this.rects.Find( x => x.Equals(selectedRect)).Width / columns;
                    cellHeight = this.rects.Find(x => x.Equals(selectedRect)).Height / rows;
                    originX = this.rects.Find(x => x.Equals(selectedRect)).OriginX;
                    originY = this.rects.Find(x => x.Equals(selectedRect)).OriginY;

                    for (double x = originX; x < (originX + this.rects.Find(z => z.Equals(selectedRect)).Width); x += cellWidth)
                    {
                        for (double y = originY; y < (originY + this.rects.Find(z => z.Equals(selectedRect)).Height); y+= cellHeight)
                        {
                            this.CreateMassiveCompartment(x,y,cellWidth,cellHeight);
                        }
                    }
                    this.rects.Remove(this.rects.Find(x => x.Equals(selectedRect)));
                    this.cnvImage.Children.Remove(this.rects.Find(x => x.Equals(selectedRect)));
                    this.cnvImage.Children.Remove((CompartmentRectangle)selectedRect);
                }
            }
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

            if (ox + w > MAX_WIDTH)
            {
                w = MAX_WIDTH - ox;
            }
            if (oy + h > MAX_HEIGHT)
            {
                h = MAX_HEIGHT - oy;
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

        private void CreateMassiveCompartment(double originx, double originy, double width, double height)
        {
            this.currentRect = new CompartmentRectangle();
            this.currentRect.SetValue(Canvas.LeftProperty, originx);
            this.currentRect.SetValue(Canvas.TopProperty, originy - 50);
            this.currentRect.Width = width;
            this.currentRect.Height = height;
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

            if (x >= MIN_WIDTH && x <= MAX_WIDTH)
            {
                ret_x = ((int)x % RESOLUTION == 0) ? (int)x : (int)x - ((int)x % RESOLUTION);
            }
            else
            {
                ret_x = (x < MIN_WIDTH) ? MIN_WIDTH : MAX_WIDTH;
            }
            return ret_x;
        }
        int NormalizeYValue(double y)
        {
            int ret_y;

            if (y >= MIN_HEIGHT && y <= MAX_HEIGHT)
            {
                ret_y = ((int)y % RESOLUTION == 0) ? (int)y : (int)y - ((int)y % RESOLUTION);
            }
            else
            {
                ret_y = (y < MIN_HEIGHT) ? MIN_HEIGHT : MAX_HEIGHT;
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
