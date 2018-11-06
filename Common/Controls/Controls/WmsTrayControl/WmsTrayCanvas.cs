using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core.Native;
using Ferretto.Common.Modules.BLL.Models;
using static Ferretto.Common.Controls.WmsRulerControl;

namespace Ferretto.Common.Controls
{
    public class WmsTrayCanvas : Canvas
    {
        #region Methods

        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(constraint);

            if (constraint.Width == double.PositiveInfinity || constraint.Height == double.PositiveInfinity)
            {
                constraint.Width = 0;
                constraint.Height = 0;
            }
            else
            {
                var parentWmsTrayControl = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is WmsTrayControl) as WmsTrayControl;

                if (parentWmsTrayControl != null && parentWmsTrayControl.TrayObject != null)
                {
                    if (this.DataContext is WmsTrayControlViewModel viewModel && constraint.Width > 0 && constraint.Height > 0)
                    {
                        double widthNewCalculated = constraint.Width;
                        double heightNewCalculated = constraint.Height;

                        //Move to Canvas Size Changed
                        //viewModel.ResizeCompartments(widthNewCalculated, heightNewCalculated);
                    }
                }
            }

            return new Size(constraint.Width, constraint.Height);
        }

        #endregion Methods
    }
}
