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
            typeof(ShowTitle),
            new PropertyMetadata(OnShowChanged));

        #endregion

        #region Methods

        public static bool GetShow(DependencyObject obj) => (bool)obj.GetValue(ShowProperty);

        public static void SetShow(DependencyObject obj, bool value) => obj.SetValue(ShowProperty, value);

        private static void OnLoadedEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is BaseEdit textEditBase && e.NewValue is bool isEnabled)
            {
                var wmsLabel = LayoutTreeHelper.GetVisualChildren(textEditBase).OfType<WmsLabel>().FirstOrDefault();
                if (wmsLabel != null)
                {
                    wmsLabel.Show(isEnabled);
                }
            }
        }

        private static void OnShowChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is BaseEdit textEditBase)
            {
                textEditBase.Loaded += (sender, eLoaded) => OnLoadedEvent(sender, e);
            }
        }

        #endregion
    }
}
