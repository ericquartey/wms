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

            var dataContext = this.DataContext as WmsTrayControlViewModel;
            if (dataContext != null && this.WidthParent > 0 && this.HeightParent > 0)
            {
                dataContext.Resize(this.WidthParent, this.HeightParent);
            }

            return new Size(this.WidthParent, this.HeightParent);
        }

        #endregion Methods
    }
}
