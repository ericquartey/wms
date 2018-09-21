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

        public CompartmentationPageView()
        {
            this.InitializeComponent();
            this.rects = new List<CompartmentRectangle>();
            this.CheckActionButtonsSelectionCorrectness();
        }

        private void ImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.currentActionMode == CompartmentActionMode.CreateCompartment)
            {
                if (this.isDrawing)
                {

                }
                else
                {
                    this.testPoint = e.GetPosition(this);
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
            if (this.currentActionMode == CompartmentActionMode.CreateCompartment)
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
                }
            }
        }

        private void ImageMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.currentActionMode == CompartmentActionMode.CreateCompartment)
            {
                if (this.isDrawing)
                {
                    this.rects.Add(this.currentRect);
                    this.currentRect = null;
                    this.isDrawing = false;
                }
            }
        }

        private void ButtonSetActionToCreateCompartment(object sender, RoutedEventArgs e)
        {
            this.SetActionToCreateCompartment();
            this.CheckActionButtonsSelectionCorrectness();
        }

        private void ButtonSetActionToDoNothing(object sender, RoutedEventArgs e)
        {
            this.SetActionToDoNothing();
            this.CheckActionButtonsSelectionCorrectness();
        }

        private void SetActionToCreateCompartment()
        {
            this.currentActionMode = CompartmentActionMode.CreateCompartment;
        }

        private void SetActionToDoNothing()
        {
            this.currentActionMode = CompartmentActionMode.DoNothing;
        }

        private void CheckActionButtonsSelectionCorrectness()
        {
            switch(this.currentActionMode)
            {
                case CompartmentActionMode.CreateCompartment:
                    this.ButtonDrawCompartments.Foreground = Brushes.Green;
                    foreach(Button btn in this.DrawButtonsStackPanel.Children)
                    {
                        if (!(btn.Name == "ButtonDrawCompartments"))
                        {
                            btn.Foreground = Brushes.White;
                        }
                    }
                    break;
                case CompartmentActionMode.DoNothing:
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
    }

    public enum CompartmentActionMode
    {
        CreateCompartment,
        DoNothing,
        NoActionSelectedYet
    }
}
