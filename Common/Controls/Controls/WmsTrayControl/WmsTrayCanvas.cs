using System.Windows;
using System.Windows.Controls;

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

            return new Size(constraint.Width, constraint.Height);
        }

        #endregion Methods
    }
}
