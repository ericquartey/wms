using System;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Interfaces;

namespace Ferretto.WMS.App
{
    public class WmsWindow : DXWindow, IWmsWindow
    {
        #region Fields

        public static readonly DependencyProperty InitialHeightProperty = DependencyProperty.Register(
            nameof(InitialHeight), typeof(double), typeof(WmsWindow), new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty InitialWidthProperty = DependencyProperty.Register(
            nameof(InitialWidth), typeof(double), typeof(WmsWindow), new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty MarginHeightProperty = DependencyProperty.Register(
            nameof(MarginHeight), typeof(double), typeof(WmsWindow), new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty MarginWidthProperty = DependencyProperty.Register(
            nameof(MarginWidth), typeof(double), typeof(WmsWindow), new UIPropertyMetadata(0.0));

        private bool isWindowLocked;

        #endregion

        #region Constructors

        public WmsWindow()
        {
            this.Loaded += this.WmsWindow_Loaded;
            this.PreviewMouseDown += this.OnPreviewMouseDown;
        }

        #endregion

        #region Properties

        public double InitialHeight
        {
            get => (double)this.GetValue(InitialHeightProperty);
            set => this.SetValue(InitialHeightProperty, value);
        }

        public double InitialWidth
        {
            get => (double)this.GetValue(InitialWidthProperty);
            set => this.SetValue(InitialWidthProperty, value);
        }

        public double MarginHeight
        {
            get => (double)this.GetValue(MarginHeightProperty);
            set => this.SetValue(MarginHeightProperty, value);
        }

        public double MarginWidth
        {
            get => (double)this.GetValue(MarginWidthProperty);
            set => this.SetValue(MarginWidthProperty, value);
        }

        #endregion

        #region Methods

        public static double AdjustSize(double startValue, double newValue, double value)
        {
            return newValue * startValue / value;
        }

        public void Lock(bool isWindowLocked)
        {
            this.isWindowLocked = isWindowLocked;
            this.ResizeMode = isWindowLocked ? ResizeMode.NoResize : ResizeMode.CanResize;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.Loaded -= this.WmsWindow_Loaded;
        }

        private void AdjustToInitialSize()
        {
            if (this.InitialWidth.Equals(0) ||
                this.InitialHeight.Equals(0))
            {
                return;
            }

            var offsetSize = FormControl.GetMainApplicationOffsetSize();

            if (offsetSize.screenWidth < this.InitialWidth)
            {
                this.Left = offsetSize.screenLeft;
                this.Top = offsetSize.screenTop;
                this.Width = offsetSize.screenWidth;
                this.Height = offsetSize.screenHeight;
                this.WindowState = WindowState.Maximized;
                return;
            }

            var widthNewCalculated = offsetSize.screenWidth - (this.MarginWidth * 2);
            var heightNewCalculated = offsetSize.screenHeight - (this.MarginHeight * 2);

            var heightConverted = AdjustSize(this.InitialHeight, widthNewCalculated, this.InitialWidth);
            if (heightConverted > heightNewCalculated)
            {
                widthNewCalculated = AdjustSize(this.InitialWidth, heightNewCalculated, this.InitialHeight);
            }
            else
            {
                heightNewCalculated = heightConverted;
            }

            this.Left = offsetSize.screenLeft + ((offsetSize.screenWidth - widthNewCalculated) / 2);
            this.Top = offsetSize.screenTop + ((offsetSize.screenHeight - heightNewCalculated) / 2);
            this.Width = widthNewCalculated;
            this.Height = heightNewCalculated;
        }

        private void OnPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.isWindowLocked)
            {
                e.Handled = LayoutTreeHelper.GetVisualParents(e.OriginalSource as DependencyObject).OfType<System.Windows.Controls.Button>()
                .FirstOrDefault(c => c.Name == "PART_CloseButton") == null;
            }
        }

        private void WmsWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.AdjustToInitialSize();
        }

        #endregion
    }
}
