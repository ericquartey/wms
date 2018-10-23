using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;
using System.Linq;

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

        //public static readonly DependencyProperty LoadingUnitProperty = DependencyProperty.Register(
        //                            nameof(LoadingUnit), typeof(LoadingUnitDetails), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLoadingUnitChanged)));

        public static readonly DependencyProperty HeightTrayProperty = DependencyProperty.Register(
                                    nameof(HeightTray), typeof(int), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHeightTrayChanged)));

        public static readonly DependencyProperty OriginXProperty = DependencyProperty.Register(
                                    nameof(OriginX), typeof(int), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOriginXChanged)));

        public static readonly DependencyProperty OriginYProperty = DependencyProperty.Register(
                                    nameof(OriginY), typeof(int), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOriginYChanged)));

        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
                    nameof(ReadOnly), typeof(bool), typeof(WmsTrayControl), new PropertyMetadata(false));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
                                    nameof(SelectedItem), typeof(CompartmentDetails), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCompartmentSelectedChanged)));

        public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(
                            nameof(ShowBackground), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnShowBackgroundChanged)));

        public static readonly DependencyProperty WidthTrayProperty = DependencyProperty.Register(
                            nameof(WidthTray), typeof(int), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnWidthTrayChanged)));

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

        public int HeightTray
        {
            get => (int)this.GetValue(HeightTrayProperty);
            set => this.SetValue(HeightTrayProperty, value);
        }

        public int OriginX
        {
            get => (int)this.GetValue(OriginXProperty);
            set => this.SetValue(OriginXProperty, value);
        }

        public int OriginY
        {
            get => (int)this.GetValue(OriginYProperty);
            set => this.SetValue(OriginYProperty, value);
        }

        public bool ReadOnly
        {
            get => (bool)this.GetValue(ReadOnlyProperty);
            set => this.SetValue(ReadOnlyProperty, value);
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

        //public LoadingUnitDetails LoadingUnit
        //{
        //    get => (LoadingUnitDetails)this.GetValue(LoadingUnitProperty);
        //    set => this.SetValue(LoadingUnitProperty, value);
        //}
        public int WidthTray
        {
            get => (int)this.GetValue(WidthTrayProperty);
            set => this.SetValue(WidthTrayProperty, value);
        }

        #endregion Properties

        #region Methods

        private static void OnCompartmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.UpdateCompartments((IEnumerable<CompartmentDetails>)e.NewValue);
            }
        }

        private static void OnCompartmentSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //DO NOTHING -> ONLY UPDATE PROPERTY
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                var newCompartment = (CompartmentDetails)e.NewValue;
                var foundCompartment = viewModel.Items.FirstOrDefault(c => c.CompartmentDetails.Id == newCompartment.Id);

                if (foundCompartment != null)
                {
                    wmsTrayControl.CanvasListBoxControl.SelectedItem = foundCompartment;
                }
            }
        }

        private static void OnHeightTrayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.TrayHeight = (int)e.NewValue;
            }
        }

        private static void OnLoadingUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.UpdateTray((LoadingUnitDetails)e.NewValue);
            }
        }

        private static void OnOriginXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                //viewModel. = (int)e.NewValue;
            }
        }

        private static void OnOriginYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.TrayWidth = (int)e.NewValue;
            }
        }

        private static void OnShowBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                wmsTrayControl.SetBackground((bool)e.NewValue);
            }
        }

        private static void OnWidthTrayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasListBoxControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.TrayWidth = (int)e.NewValue;
            }
        }

        private void SetBackground(bool show)
        {
            if (show)
            {
                var DrawingBrush = new DrawingBrush();
                DrawingBrush.TileMode = TileMode.Tile;
                DrawingBrush.Viewport = new Rect(0, 0, 25, 25);
                DrawingBrush.ViewportUnits = BrushMappingMode.Absolute;

                var gGroup = new GeometryGroup();
                gGroup.Children.Add(new RectangleGeometry(new System.Windows.Rect(0, 0, 50, 50)));
                var drawingPen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.White
                    , 1);
                var checkers = new GeometryDrawing((SolidColorBrush)new BrushConverter().ConvertFrom("#BDBDBD"), drawingPen, gGroup);
                var checkersDrawingGroup = new DrawingGroup();
                checkersDrawingGroup.Children.Add(checkers);
                DrawingBrush.Drawing = checkersDrawingGroup;

                this.Background = DrawingBrush;
            }
            else
            {
                this.Background = null;
            }
        }

        #endregion Methods
    }
}
