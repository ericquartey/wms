using System.Windows;
using System.Windows.Controls;

namespace Ferretto.WMS.App.Controls
{
    public class NotifyControl : ContentControl
    {
        #region Fields

        public static readonly DependencyProperty SubTitleDimensionProperty = DependencyProperty.Register(
            nameof(SubTitleDimension), typeof(double), typeof(NotifyControl), new FrameworkPropertyMetadata(600.0));

        #endregion

        #region Properties

        public double SubTitleDimension
        {
            get => (double)this.GetValue(SubTitleDimensionProperty);
            set => this.SetValue(SubTitleDimensionProperty, value);
        }

        #endregion

        #region Methods

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (sizeInfo == null)
            {
                return;
            }

            this.SubTitleDimension = sizeInfo.NewSize.Width;
        }

        #endregion
    }
}
