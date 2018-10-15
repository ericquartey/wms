using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core.Native;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    public class WmsTrayCanvas : Canvas
    {
        #region Properties

        public double HeightParent { get; set; }
        public double WidthParent { get; set; }

        #endregion Properties

        #region Methods

        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(constraint);

            var parentWmsTrayControl = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is WmsTrayControl) as WmsTrayControl;
            var parentWmsCanvasItemControl = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is WmsCanvasItemsControl) as WmsCanvasItemsControl;

            //if (parentWmsTrayControl != null)
            //{
            //    this.HeightParent = parentWmsTrayControl.ActualHeight;
            //    this.WidthParent = parentWmsTrayControl.ActualWidth;

            //    if (this.DataContext is WmsTrayControlViewModel viewModel && this.WidthParent > 0 && this.HeightParent > 0)
            //    {
            //        //viewModel.Resize(parentWmsCanvasItemControl, this.WidthParent, this.HeightParent);
            //    }
            //}

            //return new Size(this.WidthParent, this.HeightParent);
            return new Size(constraint.Width, constraint.Height);
        }

        #endregion Methods
    }
}
