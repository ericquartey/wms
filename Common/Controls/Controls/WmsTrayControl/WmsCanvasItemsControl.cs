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
                var heightNewCalculated = ConvertMillimetersToPixel(this.loadingUnitDetails.Length, widthNewCalculated, this.loadingUnitDetails.Width);

                this.canvas.Width = widthNewCalculated;
                this.canvas.Height = heightNewCalculated;
            }
        }

        private void WmsCanvasItemsControl_SizeChanged(Object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.canvas == null)
            {
                this.canvas = LayoutTreeHelper.GetVisualChildren(this).OfType<WmsTrayCanvas>().FirstOrDefault();
            }

            if (this.loadingUnitDetails != null)
            {
                var widthNewCalculated = this.ActualWidth;
                var heightNewCalculated = ConvertMillimetersToPixel(this.loadingUnitDetails.Length, widthNewCalculated, this.loadingUnitDetails.Width);

                if (heightNewCalculated > this.ActualHeight)
                {
                    heightNewCalculated = this.ActualHeight;
                    widthNewCalculated = ConvertMillimetersToPixel(this.loadingUnitDetails.Width, heightNewCalculated, this.loadingUnitDetails.Length);
                }
                this.canvas.Height = heightNewCalculated;
                this.canvas.Width = widthNewCalculated;
            }
        }

        #endregion Methods
    }
}
