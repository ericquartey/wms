using System.Linq;
using System.Windows;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Editors;

namespace Ferretto.Common.Controls
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

        public static bool GetShow(DependencyObject obj) => (bool)obj.GetValue(ShowProperty);

        public static void SetShow(DependencyObject obj, bool value) => obj.SetValue(ShowProperty, value);

        #endregion
    }
}
