using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DevExpress.Mvvm.UI;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    public class WmsCanvasItemsControl : ItemsControl
    {
        #region Fields

        private WmsTrayCanvas canvas;
        private LoadingUnitDetails loadingUnitDetails;
        private int offsetTray = 50;

        #endregion Fields

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.SizeChanged += this.WmsCanvasItemsControl_SizeChanged;
            this.Loaded += this.WmsCanvasItemsControl_Loaded;
        }

        private static double ConvertMillimetersToPixel(double value, double pixel, double mm, int offsetMM = 0)
        {
            if (mm > 0)
            {
                return (pixel * value) / mm + offsetMM;
            }
            return value;
        }

        private void WmsCanvasItemsControl_Loaded(Object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is WmsTrayControlViewModel wmsTrayControlViewModel)
            {
                this.loadingUnitDetails = wmsTrayControlViewModel.LoadingUnitProperty;

                var widthNewCalculated = this.ActualWidth;

                this.canvas.Width = widthNewCalculated;// - this.offsetTray;
                this.canvas.Height = ConvertMillimetersToPixel(this.loadingUnitDetails.Length, widthNewCalculated, this.loadingUnitDetails.Width);
                //this.canvas.HeightParent = this.ActualWidth;// - this.offsetTray;
                //this.canvas.WidthParent = ConvertMillimetersToPixel(this.loadingUnitDetails.Length, this.canvas.ActualWidth, this.loadingUnitDetails.Width);

                //int x = 200;
                //this.canvas.Width = x;
                //this.canvas.Height = x;
                //this.canvas.HeightParent = x;
                //this.canvas.WidthParent = x;
            }
        }

        private void WmsCanvasItemsControl_SizeChanged(Object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.canvas == null)
            {
                this.canvas = LayoutTreeHelper.GetVisualChildren(this).OfType<WmsTrayCanvas>().FirstOrDefault();
            }
            bool widthB = true;
            if (this.loadingUnitDetails != null)
            {
                var w = this.ActualWidth;// - this.offsetTray;
                var h = ConvertMillimetersToPixel(this.loadingUnitDetails.Length, w, this.loadingUnitDetails.Width);
                //this.canvas.HeightParent = this.ActualWidth - this.offsetTray;
                //this.canvas.WidthParent = ConvertMillimetersToPixel(this.loadingUnitDetails.Length, this.canvas.ActualWidth, this.loadingUnitDetails.Width);
                if (h > this.ActualHeight)
                {
                    widthB = false;
                    var hh = this.ActualHeight;// - this.offsetTray;
                    this.canvas.Height = hh;
                    this.canvas.Width = ConvertMillimetersToPixel(this.loadingUnitDetails.Width, hh, this.loadingUnitDetails.Length);// - this.offsetTray;
                    //this.canvas.HeightParent = this.ActualHeight - this.offsetTray;
                    //this.canvas.WidthParent = ConvertMillimetersToPixel(this.loadingUnitDetails.Width, this.canvas.ActualHeight, this.loadingUnitDetails.Length) - this.offsetTray;
                }
                else
                {
                    this.canvas.Height = h;
                    this.canvas.Width = w;
                }
                Console.WriteLine($"width: {this.canvas.Width} height: {this.canvas.Height}  HEIGHT({h}, {this.ActualHeight}) bool={widthB}");
            }
        }

        #endregion Methods
    }
}
