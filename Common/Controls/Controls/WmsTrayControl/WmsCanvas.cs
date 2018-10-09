using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    public class WmsCanvas : Canvas
    {
        //public static readonly DependencyProperty MouseUpCommandProperty = DependencyProperty.RegisterAttached(
        //    "MouseUpCommand", typeof(ICommand), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseUpCommandChanged)));

        #region Constructors

        public WmsCanvas()
        {
            Loaded += this.Canvas_Loaded;
        }

        #endregion Constructors

        #region Methods

        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(constraint);
            var width_parent = (float)base.ActualWidth;
            var height_parent = (float)base.ActualHeight;

            if (Double.IsNaN(width_parent))
            {
                width_parent = 0;
            }
            if (Double.IsNaN(height_parent))
            {
                height_parent = 0;
            }

            var dataContext = this.DataContext as WmsTrayControlViewModel;
            if (dataContext != null && width_parent > 0 && height_parent > 0)
            {
                float ratio = width_parent / height_parent;
                Console.WriteLine("Resize:misure override");
                dataContext.Resize(ratio);
            }

            return new Size(width_parent, height_parent);
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #endregion Methods
    }
}
