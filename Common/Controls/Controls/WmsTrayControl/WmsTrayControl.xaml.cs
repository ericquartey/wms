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

        public static readonly DependencyProperty IsCompartmentSelectableProperty = DependencyProperty.Register(
                            nameof(IsCompartmentSelectable), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsCompartmentSelectableChanged)));

        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
                            nameof(ReadOnly), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnReadOnlysChanged)));

        public static readonly DependencyProperty SelectedColorFilterFuncProperty = DependencyProperty.Register(
                                    nameof(SelectedColorFilterFunc), typeof(Func<CompartmentDetails, CompartmentDetails, Color>), typeof(WmsTrayControl),
                    new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedColorFilterFuncChanged)));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
                                    nameof(SelectedItem), typeof(CompartmentDetails), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedCompartmentChanged)));

        public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(
                            nameof(ShowBackground), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnShowBackgroundChanged)));

        public static readonly DependencyProperty ShowRulerProperty = DependencyProperty.Register(
                            nameof(ShowRuler), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnShowRulerChanged)));

        public static readonly DependencyProperty TrayProperty = DependencyProperty.Register(
                            nameof(Tray), typeof(Tray), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTrayChanged)));

        private readonly int STEP = 100;
        private BindingList<CompartmentDetails> compartments;

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

        public BindingList<CompartmentDetails> Compartments
        {
            get { return this.compartments; }
            set { this.SetValue(CompartmentsProperty, value); }
        }

        public bool IsCompartmentSelectable
        {
            get => (bool)this.GetValue(IsCompartmentSelectableProperty);
            set => this.SetValue(IsCompartmentSelectableProperty, value);
        }

        public bool ReadOnly
        {
            get => (bool)this.GetValue(ReadOnlyProperty);
            set => this.SetValue(ReadOnlyProperty, value);
        }

        public Func<CompartmentDetails, CompartmentDetails, Color> SelectedColorFilterFunc
        {
            get { return (Func<CompartmentDetails, CompartmentDetails, Color>)this.GetValue(SelectedColorFilterFuncProperty); }
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
            if (show.HasValue && show.Value || this.ShowBackground)
            {
                var drawingBrush = new DrawingBrush();
                drawingBrush.TileMode = TileMode.Tile;

                double width = 0;
                if (widthTrayPixel == 0)
                {
                    width = this.CanvasListBoxControl.Canvas.ActualWidth;
                }
                else
                {
                    width = widthTrayPixel;
                }

                double stepPixel = GraphicUtils.ConvertMillimetersToPixel(this.STEP, width, this.Tray.Dimension.Width);

                drawingBrush.Viewport = new Rect(0, 0, stepPixel, stepPixel);
                drawingBrush.ViewportUnits = BrushMappingMode.Absolute;

                var gGroup = new GeometryGroup();
                gGroup.Children.Add(new RectangleGeometry(new System.Windows.Rect(0, 0, stepPixel, stepPixel)));
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

        public void SetRuler(bool? show)
        {
            if (show.HasValue && show.Value || this.ShowBackground)
            {
                this.horizontalRuler.Visibility = Visibility.Visible;
                this.verticalRuler.Visibility = Visibility.Visible;
                this.UnitMetric.Visibility = Visibility.Visible;
                this.CanvasListBoxControl.Margin = new Thickness { Top = 25, Left = 25, Right = 1, Bottom = 1 };
            }
            else
            {
                this.horizontalRuler.Visibility = Visibility.Collapsed;
                this.verticalRuler.Visibility = Visibility.Collapsed;
                this.UnitMetric.Visibility = Visibility.Collapsed;
                this.CanvasListBoxControl.Margin = new Thickness { Top = 0, Left = 0, Right = 1, Bottom = 1 };
            }
        }

        public void UpdateRulers(double widthNewCalculated, double heightNewCalculated)
        {
            if (this.horizontalRuler.Origin == null)
            {
                this.horizontalRuler.Origin = this.Tray.Origin;
                this.verticalRuler.Origin = this.Tray.Origin;
            }
            this.horizontalRuler.Width = widthNewCalculated;
            this.verticalRuler.Height = heightNewCalculated;

            var majorIntervalStepHorizontal = this.horizontalRuler.MajorIntervalHorizontal;
            var majorIntervalStepVertical = this.verticalRuler.MajorIntervalVertical;
            this.horizontalRuler.MajorIntervalHorizontalPixel =
                (int)Math.Floor(GraphicUtils.ConvertMillimetersToPixel(majorIntervalStepHorizontal, widthNewCalculated, this.Tray.Dimension.Width));
            this.verticalRuler.MajorIntervalVerticalPixel =
                (int)Math.Floor(GraphicUtils.ConvertMillimetersToPixel(majorIntervalStepVertical, widthNewCalculated, this.Tray.Dimension.Width));
            Debug.WriteLine($"Ruler: pixel->W={widthNewCalculated} H={heightNewCalculated} IPH={this.horizontalRuler.MajorIntervalHorizontalPixel} IPV={this.horizontalRuler.MajorIntervalVerticalPixel} TRAY: real->W={this.Tray.Dimension.Width}");

            this.SetBackground(this.ShowBackground, widthNewCalculated);
        }

        private static void OnCompartmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.UpdateCompartments((IEnumerable<CompartmentDetails>)e.NewValue);
            }
        }

        private static void OnIsCompartmentSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                Debug.WriteLine($"IsSelectable Property: {e.NewValue}");

                viewModel.IsCompartmentSelectable = (bool)e.NewValue;
            }
        }

        private static void OnReadOnlysChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                Debug.WriteLine($"ReadOnly Property: {e.NewValue}");
                viewModel.UpdateReadOnlyPropertyToCompartments((bool)e.NewValue);
            }
        }

        private static void OnSelectedColorFilterFuncChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.SelectedColorFilterFunc = (Func<CompartmentDetails, CompartmentDetails, Color>)e.NewValue;
            }
        }

        /// <summary>
        /// CompartmentsProperty: Property Changed Callback, do nothing, only update the Property
        /// </summary>
        private static void OnSelectedCompartmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                var newCompartment = (CompartmentDetails)e.NewValue;
                if (viewModel.Items != null && newCompartment != null)
                {
                    var foundCompartment = viewModel.Items.FirstOrDefault(c => c.CompartmentDetails.Id == newCompartment.Id);

                    if (foundCompartment != null)
                    {
                        wmsTrayControl.CanvasListBoxControl.SelectedItem = foundCompartment;
                        viewModel.SelectedCompartment = foundCompartment.CompartmentDetails;
                    }
                }
                else
                {
                    wmsTrayControl.CanvasListBoxControl.SelectedItem = newCompartment;
                    viewModel.SelectedCompartment = newCompartment;
                }
            }
        }

        private static void OnShowBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                wmsTrayControl.SetBackground((bool)e.NewValue);
            }
        }

        private static void OnShowRulerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                wmsTrayControl.SetRuler((bool)e.NewValue);
            }
        }

        private static void OnTrayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                wmsTrayControl.Initialize();
                viewModel.Tray = (Tray)e.NewValue;

                wmsTrayControl.CanvasListBoxControl.SetControlSize(wmsTrayControl.CanvasListBoxControl.ActualHeight, wmsTrayControl.CanvasListBoxControl.ActualWidth);
            }
        }

        private void Initialize()
        {
            this.CanvasListBoxControl.Tray = this.Tray;
        }

        #endregion Methods
    }
}
