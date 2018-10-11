using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.VerticalWarehousesApp.Views
{
    public enum CompartmentActionMode
    {
        DrawCompartment,
        CreateCompartment,
        NoActionSelectedYet
    }

    public partial class CompartmentationPageView : Page
    {
        #region Fields

        private const int HEIGHT_CORRECTION = 50; //correction due to parent position
        private const int MAX_HEIGHT = 580, MIN_HEIGHT = 65;
        private const int MAX_WIDTH = 760, MIN_WIDTH = 20;
        private const int RESOLUTION = 5; //smaller drawing step

        private CompartmentActionMode currentActionMode = CompartmentActionMode.NoActionSelectedYet;

        private CompartmentRectangle currentRect;

        private Point currentRectStartPoint;

        private bool isDrawing = false;

        private List<CompartmentRectangle> rects;

        private Point testPoint;

        #endregion Fields

        #region Constructors

        public CompartmentationPageView()
        {
            this.InitializeComponent();
            this.rects = new List<CompartmentRectangle>();
            this.CheckActionButtonsSelectionCorrectness();
        }

        #endregion Constructors

        #region Methods

        private void ButtonDoDeleteCompartment(object sender, RoutedEventArgs e)
        {
            this.DeleteCompartment();
        }

        private void ButtonDoMassiveDivideCompartment(object sender, RoutedEventArgs e)
        {
            this.MassiveDivideCompartment(5, 5);
        }

        private void ButtonSetActionToCreateCompartment(object sender, RoutedEventArgs e)
        {
            this.SetActionToCreateCompartment();
            this.CheckActionButtonsSelectionCorrectness();
            this.CreateCompartment(100, 100, 100, 120);
        }

        private void ButtonSetActionToDrawCompartment(object sender, RoutedEventArgs e)
        {
            this.SetActionToDrawCompartment();
            this.CheckActionButtonsSelectionCorrectness();
        }

        private void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.currentActionMode == CompartmentActionMode.DrawCompartment)
            {
                if (this.isDrawing)
                {
                }
                else
                {
                    this.testPoint.X = this.NormalizeXValue(e.GetPosition(this).X);
                    this.testPoint.Y = this.NormalizeYValue(e.GetPosition(this).Y);

                    this.currentRectStartPoint = this.testPoint;
                    this.currentRect = new CompartmentRectangle();
                    this.currentRect.OriginX = this.testPoint.X;
                    this.currentRect.OriginY = this.testPoint.Y;
                    this.currentRect.Height = 0;
                    this.currentRect.Width = 0;
                    this.cnvImage.Children.Add(this.currentRect);

                    this.currentRect.SetValue(Canvas.LeftProperty, this.currentRect.OriginX);
                    this.currentRect.SetValue(Canvas.TopProperty, this.currentRect.OriginY); //correction due to the position of the parent canvas.

                    this.isDrawing = true;
                }
            }
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
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

        private void CanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.currentActionMode == CompartmentActionMode.DrawCompartment)
            {
                if (this.isDrawing)
                {
                    if (!this.IsCurrentCompartmentRectangleOverlappingAnotherCompRect(this.currentRect))
                    {
                        this.rects.Add(this.currentRect);
                    }
                    else
                    {
                        this.cnvImage.Children.Remove(this.currentRect);
                    }
                    this.currentRect = null;
                    this.isDrawing = false;
                }
            }
        }

        private void CheckActionButtonsSelectionCorrectness()
        {
            switch (this.currentActionMode)
            {
                case CompartmentActionMode.DrawCompartment:
                    this.ButtonDrawCompartments.Foreground = Brushes.Green;
                    foreach (Button btn in this.DrawButtonsStackPanel.Children)
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
            this.currentRect.OriginX = ox;
            this.currentRect.OriginY = oy;
            this.currentRect.SetValue(Canvas.LeftProperty, this.currentRect.OriginX);
            this.currentRect.SetValue(Canvas.TopProperty, this.currentRect.OriginY); //correction due to the position of the parent canvas.
            this.currentRect.Width = w;
            this.currentRect.Height = h;
            if (!this.IsCurrentCompartmentRectangleOverlappingAnotherCompRect(this.currentRect))
            {
                this.cnvImage.Children.Add(this.currentRect);
                this.rects.Add(this.currentRect);
            }
            this.currentRect = null;
        }

        private void CreateMassiveCompartment(double originx, double originy, double width, double height)
        {
            this.currentRect = new CompartmentRectangle();
            this.currentRect.OriginX = originx;
            this.currentRect.OriginY = originy + 50; //correction due to value being already corrected for parent Rectangle
            this.currentRect.SetValue(Canvas.LeftProperty, this.currentRect.OriginX);
            this.currentRect.SetValue(Canvas.TopProperty, this.currentRect.OriginY);
            this.currentRect.Width = width;
            this.currentRect.Height = height;
            this.cnvImage.Children.Add(this.currentRect);
            this.rects.Add(this.currentRect);
            this.currentRect = null;
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

        private bool IsCurrentCompartmentRectangleOverlappingAnotherCompRect(CompartmentRectangle cr)
        {
            if (this.rects.Count > 0)
            {
                Point tmpPoint_1 = new Point(cr.OriginX, cr.OriginY); //up left
                Point tmpPoint_2 = new Point(cr.OriginX, cr.OriginY + cr.Height); //bottom left
                Point tmpPoint_3 = new Point(cr.OriginX + cr.Width, cr.OriginY + cr.Height); //bottom right
                Point tmpPoint_4 = new Point(cr.OriginX + cr.Width, cr.OriginY); //top right
                Point centerPoint = PlanarGeometryMethods.CalculateCompartmentRectCenterPoint(cr);
                foreach (var compare_rect in this.rects)
                { //check the four angles
                    if (compare_rect.Contains(tmpPoint_1) || compare_rect.Contains(tmpPoint_2) || compare_rect.Contains(tmpPoint_3) || compare_rect.Contains(tmpPoint_4) || compare_rect.ContainsOrOnFrontier(centerPoint))
                    {
                        return true;
                    }//check the four edges
                    Point compareCompRectOrig = new Point(compare_rect.OriginX, compare_rect.OriginY);
                    Point compareCompRectFinal = new Point(compare_rect.OriginX + compare_rect.Width, compare_rect.OriginY + compare_rect.Height);
                    if (PlanarGeometryMethods.AreGivenCompartmentRectanglesOverlapping(tmpPoint_1, tmpPoint_3, compareCompRectOrig, compareCompRectFinal))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void MassiveDivideCompartment(int rows, int columns)
        {
            var selectedRect = Keyboard.FocusedElement;
            double cellWidth, cellHeight, originX, originY;
            if (selectedRect != null)
            {
                if (selectedRect.GetType() == typeof(CompartmentRectangle))
                {
                    cellWidth = this.rects.Find(x => x.Equals(selectedRect)).Width / columns;
                    cellHeight = this.rects.Find(x => x.Equals(selectedRect)).Height / rows;
                    originX = this.rects.Find(x => x.Equals(selectedRect)).OriginX;
                    originY = this.rects.Find(x => x.Equals(selectedRect)).OriginY;

                    for (double x = originX; x < (originX + this.rects.Find(z => z.Equals(selectedRect)).Width); x += cellWidth)
                    {
                        for (double y = originY; y < (originY + this.rects.Find(z => z.Equals(selectedRect)).Height); y += cellHeight)
                        {
                            this.CreateMassiveCompartment(x, y, cellWidth, cellHeight);
                        }
                    }
                    this.rects.Remove(this.rects.Find(x => x.Equals(selectedRect)));
                    this.cnvImage.Children.Remove(this.rects.Find(x => x.Equals(selectedRect)));
                    this.cnvImage.Children.Remove((CompartmentRectangle)selectedRect);
                }
            }
        }

        private int NormalizeXValue(double x)
        {
            if (x >= MIN_WIDTH && x <= MAX_WIDTH)
            {
                return ((int)x % RESOLUTION == 0) ? (int)x : (int)x - ((int)x % RESOLUTION);
            }
            else
            {
                return (x < MIN_WIDTH) ? MIN_WIDTH : MAX_WIDTH;
            }
        }

        private int NormalizeYValue(double y)
        {
            if (y >= MIN_HEIGHT && y <= MAX_HEIGHT)
            {
                return ((int)y % RESOLUTION == 0) ? (int)y : (int)y - ((int)y % RESOLUTION);
            }
            else
            {
                return (y < MIN_HEIGHT) ? MIN_HEIGHT : MAX_HEIGHT;
            }
        }

        private void SetActionToCreateCompartment()
        {
            this.currentActionMode = CompartmentActionMode.CreateCompartment;
        }

        private void SetActionToDrawCompartment()
        {
            this.currentActionMode = CompartmentActionMode.DrawCompartment;
        }

        #endregion Methods
    }
}
