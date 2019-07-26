using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Ferretto.WMS.App.Controls
{
    public class NavMenuFlyoutContentControl : ContentControl
    {
        #region Methods

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var leftDescriptor =
                DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(NavMenuFlyoutContentControl));
            leftDescriptor.AddValueChanged(this, this.OnPositionChanged);
        }

        private void OnPositionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() =>
                {
                    Canvas.SetLeft(this, 1);
                    this.Width = Application.Current.MainWindow.Width - 2;
                }));
        }

        #endregion
    }
}
