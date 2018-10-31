using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class WmsCanvasListBoxControl : System.Windows.Controls.ListBox
    {
        #region Fields

        private Brush backgroundCanvas;
        private WmsTrayCanvas canvas;
        private Position originTray;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public WmsCanvasListBoxControl()
        {
        }

        #endregion Constructors

        #region Properties

        public Brush BackgroundCanvas
        {
            get => this.backgroundCanvas;
            set
            {
                this.backgroundCanvas = value;
                if (this.canvas != null)
                {
                    this.canvas.Background = this.backgroundCanvas;
                }
            }
        }

        public WmsTrayCanvas Canvas { get => this.canvas; private set { } }
        public WmsTrayControl TrayControl { get; set; }

        #endregion Properties

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.SizeChanged += this.WmsCanvasItemsControl_SizeChanged;
            this.Loaded += this.WmsCanvasItemsControl_Loaded;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (e.AddedItems.Count > 0 && e.AddedItems[0] is WmsCompartmentViewModel newCompartment)
            {
                this.TrayControl.SelectedItem = newCompartment.CompartmentDetails;

                newCompartment.ColorFill = Colors.Blue.ToString();
                newCompartment.ColorBorder = Colors.DarkBlue.ToString();
                newCompartment.RectangleBorderThickness = 3;
            }
            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is WmsCompartmentViewModel oldCompartment)
            {
                oldCompartment.ColorFill = Colors.Aquamarine.ToString();
                oldCompartment.ColorBorder = Colors.GreenYellow.ToString();
                oldCompartment.RectangleBorderThickness = 1;
            }
        }

        private void WmsCanvasItemsControl_Loaded(Object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is WmsTrayControlViewModel wmsTrayControlViewModel)
            {
                this.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                this.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);

                this.tray = wmsTrayControlViewModel.Tray;

                var widthNewCalculated = this.ActualWidth;
                var heightNewCalculated = GraphicUtils.ConvertMillimetersToPixel(this.tray.Dimension.Height, widthNewCalculated, this.tray.Dimension.Width);

                this.canvas.Width = widthNewCalculated;
                this.canvas.Height = heightNewCalculated;

                this.originTray = this.tray.Origin;
            }
        }

        private void WmsCanvasItemsControl_SizeChanged(Object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.canvas == null)
            {
                this.canvas = LayoutTreeHelper.GetVisualChildren(this).OfType<WmsTrayCanvas>().FirstOrDefault();
            }

            if (this.tray != null)
            {
                var widthNewCalculated = this.ActualWidth;
                var heightNewCalculated = GraphicUtils.ConvertMillimetersToPixel(this.tray.Dimension.Height, widthNewCalculated, this.tray.Dimension.Width);

                if (heightNewCalculated > this.ActualHeight)
                {
                    heightNewCalculated = this.ActualHeight;
                    widthNewCalculated = GraphicUtils.ConvertMillimetersToPixel(this.tray.Dimension.Width, heightNewCalculated, this.tray.Dimension.Height);
                }
                this.canvas.Height = heightNewCalculated;
                this.canvas.Width = widthNewCalculated;

                var parentWmsTrayControl = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is WmsTrayControl) as WmsTrayControl;

                //Ruler Settings
                if (parentWmsTrayControl != null)
                {
                    parentWmsTrayControl.UpdateChildren(widthNewCalculated, heightNewCalculated);
                }

                this.canvas.Background = this.BackgroundCanvas;
            }
        }

        #endregion Methods
    }
}
