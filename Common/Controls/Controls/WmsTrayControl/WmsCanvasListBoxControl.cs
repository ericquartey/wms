using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Modules.BLL.Models;
using static Ferretto.Common.Controls.WmsRulerControl;

namespace Ferretto.Common.Controls
{
    public class WmsCanvasListBoxControl : System.Windows.Controls.ListBox
    {
        #region Fields

        private Brush backgroundCanvas;
        private Border border;
        private WmsTrayCanvas canvas;

        #endregion Fields

        #region Constructors

        public WmsCanvasListBoxControl()
        {
        }

        #endregion Constructors

        #region Properties

        public Brush BackgroundCanvas
        {
            get
            {
                return this.backgroundCanvas;
            }
            set
            {
                this.backgroundCanvas = value;
                if (this.canvas != null)
                {
                    this.canvas.Background = this.backgroundCanvas;
                }
            }
        }

        public WmsTrayCanvas Canvas { get { return this.canvas; } private set { } }

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

            Debug.WriteLine($"Canvas: pixel->W={widthNewCalculated} H={heightNewCalculated}");
        }

        public void SetControlSize(double heightNewCalculated, double widthNewCalculated)
        {
            if (this.Tray != null)
            {
                //widthNewCalculated = this.ActualWidth;
                var heightConverted = GraphicUtils.ConvertMillimetersToPixel(this.Tray.Dimension.Height, widthNewCalculated, this.Tray.Dimension.Width);

                //this.border.Height = heightNewCalculated;// + this.Tray.DOUBLE_BORDER_TRAY;
                //this.border.Width = widthNewCalculated;// + this.Tray.DOUBLE_BORDER_TRAY;

                if (heightConverted > heightNewCalculated)
                {
                    //heightNewCalculated = this.ActualHeight;
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
        }

        public void SetRulerSize(double heightNewCalculated, double widthNewCalculated)
        {
            //Ruler Settings
            var parentWmsTrayControl = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is WmsTrayControl) as WmsTrayControl;
            if (parentWmsTrayControl != null && this.Tray != null)
            {
                parentWmsTrayControl.UpdateRulers(widthNewCalculated, heightNewCalculated);
            }
        }

        public void UpdateSizeCanvas(Dimension dimension)
        {
            this.canvas.Width = dimension.Width;
            this.canvas.Height = dimension.Height;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (e.AddedItems.Count > 0 && e.AddedItems[0] is WmsCompartmentViewModel newCompartment)
            {
                this.TrayControl.SelectedItem = newCompartment.CompartmentDetails;
            }
        }

        private void WmsCanvasItemsControl_Loaded(Object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is WmsTrayControlViewModel wmsTrayControlViewModel)
            {
                this.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                this.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            }
        }

        private void WmsCanvasItemsControl_SizeChanged(Object sender, System.Windows.SizeChangedEventArgs e)
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

            Size size = e.NewSize;
            widthNewCalculated = size.Width;
            heightNewCalculated = size.Height;
            this.SetControlSize(heightNewCalculated, widthNewCalculated);
        }

        #endregion Methods
    }
}
