using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DevExpress.Xpf.Core;
using WpfScreenHelper;

namespace Ferretto.WMS.App
{
    public class WmsWindow : DXWindow
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

        #endregion

        #region Constructors

        public WmsWindow()
        {
            this.Loaded += this.WmsWindow_Loaded;
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

            var interopHelper = new WindowInteropHelper(System.Windows.Application.Current.MainWindow);
            var activeScreen = Screen.FromHandle(interopHelper.Handle);
            var area = activeScreen.WorkingArea;
            var scaledFactor = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            var screenLeft = area.Left / scaledFactor;
            var screenTop = area.Top / scaledFactor;

            var screenWidth = area.Width / scaledFactor;
            var screenHeight = area.Height / scaledFactor;

            if (screenWidth < this.InitialWidth)
            {
                this.Left = screenLeft;
                this.Top = screenTop;
                this.Width = screenWidth;
                this.Height = screenHeight;
                this.WindowState = WindowState.Maximized;
                return;
            }

            var widthNewCalculated = screenWidth - (this.MarginWidth * 2);
            var heightNewCalculated = screenHeight - (this.MarginHeight * 2);

            var heightConverted = AdjustSize(this.InitialHeight, widthNewCalculated, this.InitialWidth);
            if (heightConverted > heightNewCalculated)
            {
                widthNewCalculated = AdjustSize(this.InitialWidth, heightNewCalculated, this.InitialHeight);
            }
            else
            {
                heightNewCalculated = heightConverted;
            }

            this.Left = screenLeft + ((screenWidth - widthNewCalculated) / 2);
            this.Top = screenTop + ((screenHeight - heightNewCalculated) / 2);
            this.Width = widthNewCalculated;
            this.Height = heightNewCalculated;
        }

        private void WmsWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.AdjustToInitialSize();
        }

        #endregion
    }
}
