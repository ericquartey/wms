using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    public class WmsCanvas : Canvas
    {
        //public static readonly DependencyProperty MouseUpCommandProperty = DependencyProperty.RegisterAttached(
        //    "MouseUpCommand", typeof(ICommand), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseUpCommandChanged)));

        #region Methods

        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(constraint);
            var h = base.ActualHeight;
            var w = base.ActualWidth;
            Grid parent = base.Parent as Grid;
            var width_parent = w;// parent.Width;
            var height_parent = h;// parent.Height;

            width_parent = 0;
            height_parent = 0;

            //var x = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is Grid);

            //if (base.InternalChildren != null && base.InternalChildren.Capacity > 0)
            //{
            //    double width = base
            //    .InternalChildren
            //    .OfType<UIElement>()
            //    .Max(i => i.DesiredSize.Width + (double)i.GetValue(Canvas.LeftProperty));

            //    double height = base
            //        .InternalChildren
            //        .OfType<UIElement>()
            //        .Max(i => i.DesiredSize.Height + (double)i.GetValue(Canvas.TopProperty));

            //IEnumerator enumerator = base.Children.GetEnumerator();
            //while (enumerator.MoveNext())
            //{
            //    Rectangle element = enumerator.Current as Rectangle;
            //    if (element != null)
            //    {
            //        if (element.Name.Equals(CompartmentDrawUserControl.NAME_RECTANGLE_DRAWER))
            //        {
            //            element.Width = 1000;
            //            element.Height = 200;
            //        }
            //    }
            //}
            //OK
            //var Children = base.Children;
            //foreach (UIElement child in Children)
            //{
            //    var r = child as Rectangle;
            //    r.Width = constraint.Width - 80;
            //}
            if (Double.IsNaN(width_parent))
            {
                width_parent = 0;
            }
            if (Double.IsNaN(height_parent))
            {
                height_parent = 0;
            }
            return new Size(width_parent, height_parent);
        }

        #endregion Methods
    }
}
