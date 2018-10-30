using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;
using System.Linq;
using System.Diagnostics;

namespace Ferretto.Common.Controls
{
    /// <summary>
    /// Interaction logic for WmsHistoryTrayControl.xaml
    /// </summary>
    public partial class WmsTrayControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty CompartmentsProperty = DependencyProperty.Register(
                    nameof(Compartments), typeof(BindingList<CompartmentDetails>), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCompartmentsChanged)));

        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
                    nameof(ReadOnly), typeof(bool), typeof(WmsTrayControl), new PropertyMetadata(false));

        public static readonly DependencyProperty SelectedColorFilterFuncProperty = DependencyProperty.Register(
                                    nameof(SelectedColorFilterFunc), typeof(Func<CompartmentDetails, Color>), typeof(WmsTrayControl),
                    new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedColorFilterFuncChanged)));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
                                    nameof(SelectedItem), typeof(CompartmentDetails), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCompartmentSelectedChanged)));

        public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(
                            nameof(ShowBackground), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnShowBackgroundChanged)));

        public static readonly DependencyProperty TrayObjectProperty = DependencyProperty.Register(
                            nameof(TrayObject), typeof(Tray), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTrayObjectChanged)));

        private readonly int BORDER = 2;

        private BindingList<CompartmentDetails> compartments;

        #endregion Fields

        #region Constructors

        public WmsTrayControl()
        {
            this.InitializeComponent();
            this.CanvasListBoxControl.DataContext = new WmsTrayControlViewModel();
            this.CanvasListBoxControl.TrayControl = this;
            this.SetBackground(this.ShowBackground);
        }

        #endregion Constructors

        #region Properties

        public BindingList<CompartmentDetails> Compartments
        {
            get { return this.compartments; }
            set { this.SetValue(CompartmentsProperty, value); }
        }

        public bool ReadOnly
        {
            get => (bool)this.GetValue(ReadOnlyProperty);
            set => this.SetValue(ReadOnlyProperty, value);
        }

        public Func<IFilter, Color> SelectedColorFilterFunc
        {
            get { return (Func<IFilter, Color>)this.GetValue(SelectedColorFilterFuncProperty); }
            set
            {
                this.SetValue(SelectedColorFilterFuncProperty, value);
            }
        }

        public CompartmentDetails SelectedItem
        {
            get => (CompartmentDetails)this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public bool ShowBackground
        {
            get => (bool)this.GetValue(ShowBackgroundProperty);
            set => this.SetValue(ShowBackgroundProperty, value);
        }

        public Tray TrayObject
        {
            get => (Tray)this.GetValue(TrayObjectProperty);
            set => this.SetValue(TrayObjectProperty, value);
        }

        #endregion Properties

        #region Methods

        public void SetBackground(bool? show)
        {
            if (show.HasValue && show.Value || this.ShowBackground)
            {
                var drawingBrush = new DrawingBrush();
                drawingBrush.TileMode = TileMode.Tile;

                int border = 2;

                int step = 100 - border;
                double stepPixel = GraphicUtils.ConvertMillimetersToPixel(step, this.CanvasListBoxControl.Canvas.ActualWidth, this.TrayObject.Dimension.Width);
                //double stepPixel = this.horizontalRuler.MajorIntervalHorizontalPixel;

                drawingBrush.Viewport = new Rect(0, 0, stepPixel, stepPixel);//25
                drawingBrush.ViewportUnits = BrushMappingMode.Absolute;

                var gGroup = new GeometryGroup();
                gGroup.Children.Add(new RectangleGeometry(new System.Windows.Rect(0, 0, stepPixel, stepPixel)));//50
                var drawingPen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.White
                    , 1);
                var checkers = new GeometryDrawing((SolidColorBrush)System.Windows.Application.Current.Resources["BorderTray"], drawingPen, gGroup);

                var checkersDrawingGroup = new DrawingGroup();
                checkersDrawingGroup.Children.Add(checkers);
                drawingBrush.Drawing = checkersDrawingGroup;

                this.CanvasListBoxControl.BackgroundCanvas = drawingBrush;
            }
            else
            {
                this.CanvasListBoxControl.BackgroundCanvas = (SolidColorBrush)System.Windows.Application.Current.Resources["TrayBackground"];
            }
        }

        public void UpdateChildren(double widthNewCalculated, double heightNewCalculated)
        {
            if (this.horizontalRuler.Origin == null)
            {
                this.horizontalRuler.Origin = this.TrayObject.Origin;
                this.verticalRuler.Origin = this.TrayObject.Origin;
            }
            this.horizontalRuler.Width = widthNewCalculated + this.BORDER;
            this.verticalRuler.Height = heightNewCalculated + this.BORDER;
            var majorIntervalStepHorizontal = this.horizontalRuler.MajorIntervalHorizontal;
            var majorIntervalStepVertical = this.verticalRuler.MajorIntervalVertical;
            this.horizontalRuler.MajorIntervalHorizontalPixel =
                (int)GraphicUtils.ConvertMillimetersToPixel(majorIntervalStepHorizontal, widthNewCalculated, this.TrayObject.Dimension.Width);
            this.verticalRuler.MajorIntervalVerticalPixel =
                (int)GraphicUtils.ConvertMillimetersToPixel(majorIntervalStepVertical, widthNewCalculated, this.TrayObject.Dimension.Width);
            Debug.WriteLine($"DRAW-RULER: TRAY: W_PIXEL={widthNewCalculated} H_PIXEL={heightNewCalculated} W={this.TrayObject.Dimension.Width}");

            //Update Grid
            this.SetBackground(this.ShowBackground);
            //Border
            this.CanvasBorder.Width = widthNewCalculated + 3;
            this.CanvasBorder.Height = heightNewCalculated + 3;
        }

        private static void OnCompartmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.UpdateCompartments((IEnumerable<CompartmentDetails>)e.NewValue);
            }
        }

        /// <summary>
        /// CompartmentsProperty: Property Changed Callback, do nothing, only update the Property
        /// </summary>
        private static void OnCompartmentSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                var newCompartment = (CompartmentDetails)e.NewValue;
                if (viewModel.Items != null)
                {
                    var foundCompartment = viewModel.Items.FirstOrDefault(c => c.CompartmentDetails.Id == newCompartment.Id);

                    if (foundCompartment != null)
                    {
                        wmsTrayControl.CanvasListBoxControl.SelectedItem = foundCompartment;
                    }
                }
            }
        }

        private static void OnSelectedColorFilterFuncChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.SelectedColorFilterFunc = (Func<CompartmentDetails, Color>)e.NewValue;
            }
        }

        private static void OnShowBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                wmsTrayControl.SetBackground((bool)e.NewValue);
            }
        }

        private static void OnTrayObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                ////////
                wmsTrayControl.Initialize();
                ////////
                viewModel.UpdateTray((Tray)e.NewValue);
            }
        }

        private void Initialize()
        {
            this.CanvasListBoxControl.Tray = this.TrayObject;
            //this.tray = wmsTrayControlViewModel.Tray;

            //if (this.tray != null)
            //{
            //var widthNewCalculated = this.ActualWidth;
            //var heightNewCalculated = GraphicUtils.ConvertMillimetersToPixel(this.TrayObject.Dimension.Height, widthNewCalculated, this.TrayObject.Dimension.Width);

            //this.canvas.Width = widthNewCalculated;
            //this.canvas.Height = heightNewCalculated;
            //this.CanvasListBoxControl.Width = widthNewCalculated;
            //this.CanvasListBoxControl.Height = heightNewCalculated;

            //this.CanvasListBoxControl.OriginTray = this.TrayObject.Origin;
            //this.originTray = this.tray.Origin;
            //}
        }

        #endregion Methods
    }
}
