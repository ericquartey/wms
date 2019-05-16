using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using Ferretto.WMS.App.Controls.Interfaces;
using WpfScreenHelper;

namespace Ferretto.WMS.App.Controls
{
    public static class FormControl
    {
        #region Methods

        public static(double screenTop, double screenLeft, double screenWidth, double screenHeight) GetMainApplicationOffsetSize()
        {
            var interopHelper = new WindowInteropHelper(System.Windows.Application.Current.MainWindow);
            var activeScreen = Screen.FromHandle(interopHelper.Handle);
            var area = activeScreen.WorkingArea;
            var scaledFactor = VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip;

            var screenLeft = area.Left / scaledFactor;
            var screenTop = area.Top / scaledFactor;

            var screenWidth = area.Width / scaledFactor;
            var screenHeight = area.Height / scaledFactor;

            return (screenTop, screenLeft, screenWidth, screenHeight);
        }

        public static void SetFocus(INavigableView view, string controlNameToFocus)
        {
            if (string.IsNullOrEmpty(controlNameToFocus) == false &&
                view is DependencyObject viewDep)
            {
                var elemToFocus = LayoutTreeHelper.GetVisualChildren(viewDep).OfType<FrameworkElement>()
                                                  .FirstOrDefault(item => item.Name == controlNameToFocus);
                if (elemToFocus != null)
                {
                    elemToFocus.Focus();
                }
            }
        }

        #endregion
    }
}
