using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DevExpress.Mvvm.UI;

namespace Ferretto.Common.Controls
{
    public class WmsCanvasItemsControl : ItemsControl
    {
        #region Fields

        private WmsTrayCanvas canvas;

        #endregion Fields

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.SizeChanged += this.WmsCanvasItemsControl_SizeChanged;
        }

        private void WmsCanvasItemsControl_SizeChanged(Object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.canvas == null)
            {
                this.canvas = LayoutTreeHelper.GetVisualChildren(this).OfType<WmsTrayCanvas>().FirstOrDefault();
            }
            this.canvas.HeightParent = this.ActualHeight;
            this.canvas.WidthParent = this.ActualWidth;
        }

        #endregion Methods
    }
}
