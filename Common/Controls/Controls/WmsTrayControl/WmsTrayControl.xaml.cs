using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;
using System.Linq;

namespace Ferretto.Common.Controls
{
    /// <summary>
    /// Interaction logic for WmsTrayControl.xaml
    /// </summary>
    public partial class WmsTrayControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty CanvasMinHeightProperty = DependencyProperty.Register(
            nameof(CanvasMinHeight), typeof(double), typeof(WmsTrayControl), new UIPropertyMetadata(150.0));

        public static readonly DependencyProperty IsCompartmentSelectableProperty = DependencyProperty.Register(
            nameof(IsCompartmentSelectable), typeof(bool), typeof(WmsTrayControl),
            new FrameworkPropertyMetadata(true, OnIsCompartmentSelectableChanged));

        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
            nameof(ReadOnly), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(OnReadOnlyChanged));

        public static readonly DependencyProperty SelectedColorFilterFuncProperty = DependencyProperty.Register(
            nameof(SelectedColorFilterFunc), typeof(Func<CompartmentDetails, CompartmentDetails, string>),
            typeof(WmsTrayControl),
            new FrameworkPropertyMetadata(OnSelectedColorFilterFuncChanged));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), typeof(CompartmentDetails), typeof(WmsTrayControl),
            new FrameworkPropertyMetadata(OnSelectedCompartmentChanged));

        public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(
            nameof(ShowBackground), typeof(bool), typeof(WmsTrayControl),
            new FrameworkPropertyMetadata(OnShowBackgroundChanged));

        public static readonly DependencyProperty ShowRulerProperty = DependencyProperty.Register(
            nameof(ShowRuler), typeof(bool), typeof(WmsTrayControl),
            new FrameworkPropertyMetadata(OnShowRulerChanged));

        public static readonly DependencyProperty TrayProperty = DependencyProperty.Register(
            nameof(Tray), typeof(Tray), typeof(WmsTrayControl), new FrameworkPropertyMetadata(OnTrayChanged));

        private readonly int STEP = 100;

        #endregion Fields

        #region Constructors

        public WmsTrayControl()
        {
            this.InitializeComponent();
            this.CanvasListBoxControl.DataContext = new WmsTrayControlViewModel();
            this.CanvasListBoxControl.TrayControl = this;
            this.SetBackground(this.ShowBackground);
            this.SetRuler(this.ShowRuler);
        }

        #endregion Constructors

        #region Properties

        public double CanvasMinHeight
        {
            get => (double) this.GetValue(CanvasMinHeightProperty);
            set => this.SetValue(CanvasMinHeightProperty, value);
        }

        public bool IsCompartmentSelectable
        {
            get => (bool) this.GetValue(IsCompartmentSelectableProperty);
            set => this.SetValue(IsCompartmentSelectableProperty, value);
        }

        public bool ReadOnly
        {
            get => (bool) this.GetValue(ReadOnlyProperty);
            set => this.SetValue(ReadOnlyProperty, value);
        }

        public Func<CompartmentDetails, CompartmentDetails, string> SelectedColorFilterFunc
        {
            get => (Func<CompartmentDetails, CompartmentDetails, string>) this.GetValue(
                SelectedColorFilterFuncProperty);
            set => this.SetValue(SelectedColorFilterFuncProperty, value);
        }

        public CompartmentDetails SelectedItem
        {
            get => (CompartmentDetails) this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public bool ShowBackground
        {
            get => (bool) this.GetValue(ShowBackgroundProperty);
            set => this.SetValue(ShowBackgroundProperty, value);
        }

        public bool ShowRuler
        {
            get => (bool) this.GetValue(ShowRulerProperty);
            set => this.SetValue(ShowRulerProperty, value);
        }

        public Tray Tray
        {
            get => (Tray) this.GetValue(TrayProperty);
            set => this.SetValue(TrayProperty, value);
        }

        #endregion Properties

        #region Methods

        public void SetBackground(bool? show, double widthTrayPixel = 0)
        {
            if (show.HasValue && show.Value || this.ShowBackground)
            {
                var drawingBrush = new DrawingBrush
                {
                    TileMode = TileMode.Tile
                };

                double width;
                if (widthTrayPixel == 0 && this.CanvasListBoxControl.Canvas != null)
                {
                    width = this.CanvasListBoxControl.Canvas.ActualWidth;
                }
                else
                {
                    width = widthTrayPixel;
                }

                var stepPixel = GraphicUtils.ConvertMillimetersToPixel(this.STEP, width, this.Tray.Dimension.Width);

                drawingBrush.Viewport = new Rect(0, 0, stepPixel, stepPixel);
                drawingBrush.ViewportUnits = BrushMappingMode.Absolute;

                var gGroup = new GeometryGroup();
                gGroup.Children.Add(new LineGeometry(new Point(stepPixel, 0), new Point(stepPixel, stepPixel)));
                gGroup.Children.Add(new LineGeometry(new Point(0, stepPixel), new Point(stepPixel, stepPixel)));
                var drawingPen = new Pen((SolidColorBrush) Application.Current.Resources["BackgroundGridBrush"], 1);
                var checkers = new GeometryDrawing((SolidColorBrush) Application.Current.Resources["TrayBackground"],
                    drawingPen, gGroup);

                var checkersDrawingGroup = new DrawingGroup();
                checkersDrawingGroup.Children.Add(checkers);
                drawingBrush.Drawing = checkersDrawingGroup;

                this.CanvasListBoxControl.BackgroundCanvas = drawingBrush;
            }
            else
            {
                this.CanvasListBoxControl.BackgroundCanvas =
                    (SolidColorBrush) Application.Current.Resources["TrayBackground"];
            }
        }

        public void SetRuler(bool? show)
        {
            if (show.HasValue && show.Value || this.ShowBackground)
            {
                this.horizontalRuler.Visibility = Visibility.Visible;
                this.verticalRuler.Visibility = Visibility.Visible;
                this.UnitMetric.Visibility = Visibility.Visible;
                this.CanvasListBoxControl.Margin = new Thickness { Top = 25, Left = 25, Right = 0, Bottom = 0 };
            }
            else
            {
                this.horizontalRuler.Visibility = Visibility.Collapsed;
                this.verticalRuler.Visibility = Visibility.Collapsed;
                this.UnitMetric.Visibility = Visibility.Collapsed;
                this.CanvasListBoxControl.Margin = new Thickness { Top = 0, Left = 0, Right = 0, Bottom = 0 };
            }
        }

        public void UpdateRulers(double widthNewCalculated, double heightNewCalculated)
        {
            if (this.horizontalRuler.Origin == null)
            {
                this.horizontalRuler.Origin = this.Tray.Origin;
                this.verticalRuler.Origin = this.Tray.Origin;
            }
            this.horizontalRuler.WidthMmForConvert = this.Tray.Dimension.Width;
            this.horizontalRuler.WidthPixelForConvert = widthNewCalculated;
            this.horizontalRuler.HeightMmForRatio = this.Tray.Dimension.Height;

            this.verticalRuler.WidthMmForConvert = this.Tray.Dimension.Width;
            this.verticalRuler.WidthPixelForConvert = widthNewCalculated;
            this.verticalRuler.HeightMmForRatio = this.Tray.Dimension.Height;

            this.horizontalRuler.Width = widthNewCalculated;
            this.verticalRuler.Height = heightNewCalculated;

            var majorIntervalStepHorizontal = this.horizontalRuler.MajorIntervalHorizontal;
            var majorIntervalStepVertical = this.verticalRuler.MajorIntervalVertical;
            this.horizontalRuler.MajorIntervalHorizontalPixel =
                (int) Math.Floor(GraphicUtils.ConvertMillimetersToPixel(majorIntervalStepHorizontal, widthNewCalculated,
                    this.Tray.Dimension.Width));
            this.verticalRuler.MajorIntervalVerticalPixel =
                (int) Math.Floor(GraphicUtils.ConvertMillimetersToPixel(majorIntervalStepVertical, widthNewCalculated,
                    this.Tray.Dimension.Width));

            this.SetBackground(this.ShowBackground, widthNewCalculated);
        }

        private static void OnCompartmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl &&
                wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.UpdateCompartments((IEnumerable<CompartmentDetails>) e.NewValue);
            }
        }

        private static void OnIsCompartmentSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl &&
                wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.IsCompartmentSelectable = (bool)e.NewValue;
            }
        }

        private static void OnReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl &&
                wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.UpdateReadOnlyPropertyToCompartments((bool) e.NewValue);
            }
        }

        private static void OnSelectedColorFilterFuncChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl &&
                wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.SelectedColorFilterFunc = (Func<CompartmentDetails, CompartmentDetails, string>) e.NewValue;
            }
        }

        /// <summary>
        /// CompartmentsProperty: Property Changed Callback, do nothing, only update the Property
        /// </summary>
        private static void OnSelectedCompartmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!( d is WmsTrayControl wmsTrayControl ) ||
                !( wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel ))
            {
                return;
            }

            var newCompartment = (CompartmentDetails) e.NewValue;
            if (viewModel.Items != null && newCompartment != null)
            {
                var foundCompartment =
                    viewModel.Items.FirstOrDefault(c => c.CompartmentDetails.Id == newCompartment.Id);

                if (foundCompartment == null)
                {
                    return;
                }

                wmsTrayControl.CanvasListBoxControl.SelectedItem = foundCompartment;
                viewModel.SelectedCompartment = foundCompartment.CompartmentDetails;
            }
            else
            {
                wmsTrayControl.CanvasListBoxControl.SelectedItem = newCompartment;
                viewModel.SelectedCompartment = newCompartment;
            }
        }

        private static void OnShowBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel && wmsTrayControl.Tray != null && wmsTrayControl.CanvasListBoxControl.Canvas != null)
            {
                wmsTrayControl.SetBackground((bool) e.NewValue);
            }
        }

        private static void OnShowRulerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl &&
                wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel)
            {
                wmsTrayControl.SetRuler((bool) e.NewValue);
            }
        }

        private static void OnTrayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!( d is WmsTrayControl wmsTrayControl ) ||
                !( wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel ))
            {
                return;
            }

            wmsTrayControl.Initialize();
            viewModel.Tray = (Tray)e.NewValue;

            if (wmsTrayControl.CanvasListBoxControl.ActualHeight > 0 && wmsTrayControl.CanvasListBoxControl.ActualWidth > 0)
            {
                wmsTrayControl.CanvasListBoxControl.SetControlSize(wmsTrayControl.CanvasListBoxControl.ActualHeight, wmsTrayControl.CanvasListBoxControl.ActualWidth);
            }

            wmsTrayControl.SetBackground(null);
        }

        private void Initialize()
        {
            this.CanvasListBoxControl.Tray = this.Tray;
        }

        #endregion Methods
    }
}
