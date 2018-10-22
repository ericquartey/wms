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
        #region Methods

        protected override Size MeasureOverride(Size constraint)
        {
            //base.MeasureOverride(constraint);

            var parentWmsTrayControl = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is WmsTrayControl) as WmsTrayControl;

            if (parentWmsTrayControl != null)
            {
                if (this.DataContext is WmsTrayControlViewModel viewModel && constraint.Width > 0 && constraint.Height > 0)
                {
                    viewModel.Resize(constraint.Width, constraint.Height);
                }
            }

            return new Size(constraint.Width, constraint.Height);
        }

        #endregion Methods
    }
}
