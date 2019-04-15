using System.Windows;

namespace Ferretto.WMS.App.Controls
{
    public class ShowTitle : DependencyObject
    {
        #region Fields

        public static readonly DependencyProperty ShowProperty = DependencyProperty.RegisterAttached(
            "Show",
            typeof(bool),
            typeof(ShowTitle));

        #endregion

        #region Methods

        public static bool GetShow(DependencyObject element) => (bool)element?.GetValue(ShowProperty);

        public static void SetShow(DependencyObject element, bool value) => element?.SetValue(ShowProperty, value);

        #endregion
    }
}
