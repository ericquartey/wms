using System;
using System.Linq;
using System.Windows;
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
        private Border border;
        private WmsTrayCanvas canvas;

        #endregion Fields

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

        public WmsTrayCanvas Canvas {
            get => this.canvas;
            private set { }
        }

        public Position OriginTray { get; set; }

        public Tray Tray { get; set; }
        public WmsTrayControl TrayControl { get; set; }

        #endregion Properties

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.SizeChanged += this.WmsCanvasItemsControl_SizeChanged;
            this.Loaded += this.WmsCanvasItemsControl_Loaded;
        }

        public void SetBorderSize(double heightNewCalculated, double widthNewCalculated)
        {
            this.border.Height = heightNewCalculated;
            this.border.Width = widthNewCalculated;
        }

        public void SetCanvasSize(double heightNewCalculated, double widthNewCalculated)
        {
            this.canvas.Height = heightNewCalculated;
            this.canvas.Width = widthNewCalculated;

            this.canvas.Background = this.BackgroundCanvas;
        }

        public void SetControlSize(double heightNewCalculated, double widthNewCalculated)
        {
            if (this.Tray == null)
            {
                return;
            }

            var heightConverted = GraphicUtils.ConvertMillimetersToPixel(this.Tray.Dimension.Height, widthNewCalculated, this.Tray.Dimension.Width);

            if (heightConverted > heightNewCalculated)
            {
                widthNewCalculated = GraphicUtils.ConvertMillimetersToPixel(this.Tray.Dimension.Width, heightNewCalculated, this.Tray.Dimension.Height);
            }
            else
            {
                heightNewCalculated = heightConverted;
            }
            heightNewCalculated = Math.Floor(heightNewCalculated);
            widthNewCalculated = Math.Floor(widthNewCalculated);

            this.SetBorderSize(heightNewCalculated, widthNewCalculated);

            heightNewCalculated -= this.Tray.DOUBLE_BORDER_TRAY;
            widthNewCalculated -= this.Tray.DOUBLE_BORDER_TRAY;

            this.SetCanvasSize(heightNewCalculated, widthNewCalculated);

            this.SetRulerSize(heightNewCalculated, widthNewCalculated);

            if (this.DataContext is WmsTrayControlViewModel wmsTrayControlViewModel)
            {
                wmsTrayControlViewModel.ResizeCompartments(widthNewCalculated, heightNewCalculated);
            }
        }

        public void SetRulerSize(double heightNewCalculated, double widthNewCalculated)
        {
            var parentWmsTrayControl = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is WmsTrayControl) as WmsTrayControl;
            if (parentWmsTrayControl != null && this.Tray != null)
            {
                parentWmsTrayControl.UpdateRulers(widthNewCalculated, heightNewCalculated);
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (e.AddedItems.Count > 0 && e.AddedItems[0] is WmsCompartmentViewModel newCompartment)
            {
                this.TrayControl.SelectedItem = newCompartment.CompartmentDetails;
            }
        }

        private void WmsCanvasItemsControl_Loaded(Object sender, RoutedEventArgs e)
        {
            if (!( this.DataContext is WmsTrayControlViewModel ))
            {
                return;
            }

            this.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            this.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
        }

        private void WmsCanvasItemsControl_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            if (this.canvas == null)
            {
                this.canvas = LayoutTreeHelper.GetVisualChildren(this).OfType<WmsTrayCanvas>().FirstOrDefault();
            }
            if (this.border == null)
            {
                this.border = LayoutTreeHelper.GetVisualChildren(this).OfType<Border>().FirstOrDefault(x => x.Name == "ListBoxBorder");
            }

            double widthNewCalculated = 0;
            double heightNewCalculated = 0;

            var size = e.NewSize;
            widthNewCalculated = size.Width;
            heightNewCalculated = size.Height;
            this.SetControlSize(heightNewCalculated, widthNewCalculated);
        }

        #endregion Methods
    }
}
