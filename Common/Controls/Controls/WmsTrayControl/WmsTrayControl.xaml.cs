using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public partial class WmsTrayControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty CanvasMinHeightProperty = DependencyProperty.Register(
            nameof(CanvasMinHeight), typeof(double), typeof(WmsTrayControl), new UIPropertyMetadata(150.0));

        public static readonly DependencyProperty IsCompartmentSelectableProperty = DependencyProperty.Register(
            nameof(IsCompartmentSelectable),
            typeof(bool),
            typeof(WmsTrayControl),
            new FrameworkPropertyMetadata(true, OnIsCompartmentSelectableChanged));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(OnIsReadOnlyChanged));

        public static readonly DependencyProperty RulerStepProperty = DependencyProperty.Register(
            nameof(RulerStep), typeof(int), typeof(WmsTrayControl), new FrameworkPropertyMetadata(100, OnRulerStepChanged));

        public static readonly DependencyProperty SelectedColorFilterFuncProperty = DependencyProperty.Register(
            nameof(SelectedColorFilterFunc),
            typeof(Func<ICompartment, ICompartment, string>),
            typeof(WmsTrayControl),
            new FrameworkPropertyMetadata(OnSelectedColorFilterFuncChanged));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(ICompartment),
            typeof(WmsTrayControl),
            new FrameworkPropertyMetadata(OnSelectedCompartmentChanged));

        public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(
            nameof(ShowBackground),
            typeof(bool),
            typeof(WmsTrayControl),
            new FrameworkPropertyMetadata(OnShowBackgroundChanged));

        public static readonly DependencyProperty ShowRulerProperty = DependencyProperty.Register(
            nameof(ShowRuler), typeof(bool), typeof(WmsTrayControl));

        public static readonly DependencyProperty TrayProperty = DependencyProperty.Register(
            nameof(Tray), typeof(Tray), typeof(WmsTrayControl), new FrameworkPropertyMetadata(OnTrayChanged));

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

        public double CanvasMinHeight
        {
            get => (double)this.GetValue(CanvasMinHeightProperty);
            set => this.SetValue(CanvasMinHeightProperty, value);
        }

        public bool IsCompartmentSelectable
        {
            get => (bool)this.GetValue(IsCompartmentSelectableProperty);
            set => this.SetValue(IsCompartmentSelectableProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)this.GetValue(IsReadOnlyProperty);
            set => this.SetValue(IsReadOnlyProperty, value);
        }

        public int RulerStep
        {
            get => (int)this.GetValue(RulerStepProperty);
            set => this.SetValue(RulerStepProperty, value);
        }

        public Func<ICompartment, ICompartment, string> SelectedColorFilterFunc
        {
            get => (Func<ICompartment, ICompartment, string>)this.GetValue(
                SelectedColorFilterFuncProperty);
            set => this.SetValue(SelectedColorFilterFuncProperty, value);
        }

        public ICompartment SelectedItem
        {
            get => (ICompartment)this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public bool ShowBackground
        {
            get => (bool)this.GetValue(ShowBackgroundProperty);
            set => this.SetValue(ShowBackgroundProperty, value);
        }

        public bool ShowRuler
        {
            get => (bool)this.GetValue(ShowRulerProperty);
            set => this.SetValue(ShowRulerProperty, value);
        }

        public Tray Tray
        {
            get => (Tray)this.GetValue(TrayProperty);
            set => this.SetValue(TrayProperty, value);
        }

        #endregion Properties

        #region Methods

        public void SetBackground(bool? show, double widthTrayPixel = 0)
        {
            if ((show.HasValue && show.Value) || this.ShowBackground)
            {
                var drawingBrush = new DrawingBrush
                {
                    TileMode = TileMode.Tile
                };

                var width = widthTrayPixel == 0 && this.CanvasListBoxControl.Canvas != null
                    ? this.CanvasListBoxControl.Canvas.ActualWidth
                    : widthTrayPixel;
                var stepPixel = GraphicUtils.ConvertMillimetersToPixel(this.RulerStep, width, this.Tray.Dimension.Width);

                drawingBrush.Viewport = new Rect(0, 0, stepPixel, stepPixel);
                drawingBrush.ViewportUnits = BrushMappingMode.Absolute;

                var gGroup = new GeometryGroup();
                gGroup.Children.Add(new LineGeometry(new Point(stepPixel, 0), new Point(stepPixel, stepPixel)));
                gGroup.Children.Add(new LineGeometry(new Point(0, stepPixel), new Point(stepPixel, stepPixel)));
                var drawingPen = new Pen((SolidColorBrush)Application.Current.Resources["BackgroundGridBrush"], 1);
                var checkers = new GeometryDrawing(
                    (SolidColorBrush)Application.Current.Resources["TrayBackground"],
                    drawingPen,
                    gGroup);

                var checkersDrawingGroup = new DrawingGroup();
                checkersDrawingGroup.Children.Add(checkers);
                drawingBrush.Drawing = checkersDrawingGroup;

                this.CanvasListBoxControl.BackgroundCanvas = drawingBrush;
            }
            else
            {
                this.CanvasListBoxControl.BackgroundCanvas =
                    (SolidColorBrush)Application.Current.Resources["TrayBackground"];
            }
        }

        public void UpdateRulers(double widthNewCalculated, double heightNewCalculated)
        {
            if (this.HorizontalRulerControl.Origin == null)
            {
                this.HorizontalRulerControl.Origin = this.Tray.Origin;
                this.VerticalRulerControl.Origin = this.Tray.Origin;
            }

            this.HorizontalRulerControl.WidthMmForConvert = this.Tray.Dimension.Width;
            this.HorizontalRulerControl.WidthPixelForConvert = widthNewCalculated;
            this.HorizontalRulerControl.HeightMmForRatio = this.Tray.Dimension.Height;

            this.VerticalRulerControl.WidthMmForConvert = this.Tray.Dimension.Width;
            this.VerticalRulerControl.WidthPixelForConvert = widthNewCalculated;
            this.VerticalRulerControl.HeightMmForRatio = this.Tray.Dimension.Height;

            this.HorizontalRulerControl.Width = widthNewCalculated;
            this.VerticalRulerControl.Height = heightNewCalculated;

            var majorIntervalStepHorizontal = this.HorizontalRulerControl.MajorInterval;
            var majorIntervalStepVertical = this.VerticalRulerControl.MajorInterval;
            this.HorizontalRulerControl.MajorIntervalPixel =
                (int)Math.Floor(GraphicUtils.ConvertMillimetersToPixel(
                                    majorIntervalStepHorizontal,
                                    widthNewCalculated,
                                    this.Tray.Dimension.Width));
            this.VerticalRulerControl.MajorIntervalPixel =
                (int)Math.Floor(GraphicUtils.ConvertMillimetersToPixel(
                                    majorIntervalStepVertical,
                                    widthNewCalculated,
                                    this.Tray.Dimension.Width));

            this.SetBackground(this.ShowBackground, widthNewCalculated);
        }

        private static void OnIsCompartmentSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl &&
                wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.IsCompartmentSelectable = (bool)e.NewValue;
            }
        }

        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl &&
                wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.UpdateIsReadOnlyPropertyToCompartments((bool)e.NewValue);
            }
        }

        private static void OnRulerStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl trayControl)
            {
                trayControl.HorizontalRulerControl.MajorInterval = 2 * (int)e.NewValue;
                trayControl.VerticalRulerControl.MajorInterval = 2 * (int)e.NewValue;
            }
        }

        private static void OnSelectedColorFilterFuncChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl &&
                wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.SelectedColorFilterFunc = (Func<ICompartment, ICompartment, string>)e.NewValue;
            }
        }

        private static void OnSelectedCompartmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is WmsTrayControl wmsTrayControl) ||
                !(wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel))
            {
                return;
            }

            viewModel.SelectedCompartment = (ICompartment)e.NewValue;
        }

        private static void OnShowBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl &&
                wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel &&
                wmsTrayControl.Tray != null && wmsTrayControl.CanvasListBoxControl.Canvas != null)
            {
                wmsTrayControl.SetBackground((bool)e.NewValue);
            }
        }

        private static void OnTrayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is WmsTrayControl wmsTrayControl) ||
                !(wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel))
            {
                return;
            }

            wmsTrayControl.Initialize();
            viewModel.Tray = (Tray)e.NewValue;

            if (wmsTrayControl.CanvasListBoxControl.ActualHeight > 0 && wmsTrayControl.CanvasListBoxControl.ActualWidth > 0)
            {
                wmsTrayControl.CanvasListBoxControl.SetControlSize(wmsTrayControl.CanvasListBoxControl.ActualHeight, wmsTrayControl.CanvasListBoxControl.ActualWidth);

                if (wmsTrayControl.CanvasListBoxControl.Canvas.ActualWidth > 0)
                {
                    wmsTrayControl.SetBackground(null, wmsTrayControl.CanvasListBoxControl.Canvas.ActualWidth);
                }
            }
        }

        private void Initialize()
        {
            this.CanvasListBoxControl.Tray = this.Tray;
        }

        #endregion Methods
    }
}
