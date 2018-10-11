using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    /// <summary>
    /// Interaction logic for WmsHistoryTrayControl.xaml
    /// </summary>
    public partial class WmsTrayControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty LoadingUnitProperty = DependencyProperty.Register(
                    nameof(LoadingUnit), typeof(LoadingUnitDetails), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLoadingUnitChanged)));

        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
                    nameof(ReadOnly), typeof(bool), typeof(WmsTrayControl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(
                    nameof(ShowBackground), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnShowBackgroundChanged)));

        #endregion Fields

        #region Constructors

        public WmsTrayControl()
        {
            this.InitializeComponent();
            this.CanvasItemsControl.DataContext = new WmsTrayControlViewModel();
            this.SetBackground(this.ShowBackground);
            

        }

        #endregion Constructors

        #region Properties

        public LoadingUnitDetails LoadingUnit
        {
            get => (LoadingUnitDetails)this.GetValue(LoadingUnitProperty);
            set => this.SetValue(LoadingUnitProperty, value);
        }

        public bool ReadOnly
        {
            get => (bool)this.GetValue(ReadOnlyProperty);
            set => this.SetValue(ReadOnlyProperty, value);
        }
        public bool ShowBackground
        {
            get => (bool)this.GetValue(ShowBackgroundProperty);
            set => this.SetValue(ShowBackgroundProperty, value);
        }

        #endregion Properties

        #region Methods

        private static void OnLoadingUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasItemsControl.DataContext is WmsTrayControlViewModel viewModel)
            {  
                viewModel.UpdateTray((LoadingUnitDetails)e.NewValue);
            }
        }

        private static void OnShowBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsTrayControl wmsTrayControl && wmsTrayControl.CanvasItemsControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                wmsTrayControl.SetBackground((bool)e.NewValue);
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
                    //(SolidColorBrush)new BrushConverter().ConvertFrom("#E0E0E0")
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
