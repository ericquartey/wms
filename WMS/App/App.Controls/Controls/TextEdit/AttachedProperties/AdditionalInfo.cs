using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Editors;

namespace Ferretto.WMS.App.Controls
{
    public class AdditionalInfo : DependencyObject
    {
        #region Fields

        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
           "Text", typeof(string), typeof(AdditionalInfo), new UIPropertyMetadata(OnTextChanged));

        #endregion

        #region Properties

        public string NewValue { get; set; }

        #endregion

        #region Methods

        public static string GetText(DependencyObject element) => (string)element?.GetValue(TextProperty);

        public static void SetText(DependencyObject element, string value) => element?.SetValue(TextProperty, value);

        private static void BaseEdit_Loaded(object sender, RoutedEventArgs e)
        {
            SetAdditionalInfo((DependencyObject)sender, false);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetAdditionalInfo(d, true);
        }

        private static void SetAdditionalInfo(DependencyObject d, bool binding)
        {
            if (!(d is IBaseEdit baseEdit))
            {
                return;
            }

            var prop = baseEdit.GetValue(TextProperty);

            var y = LayoutTreeHelper.GetVisualParents(baseEdit.EditCore)
                .OfType<Grid>()
                .FirstOrDefault(x => x.Name == "TextEditGrid");
            if (y != null)
            {
                var wmsLabel = y.Children.OfType<WmsLabel>().FirstOrDefault(x => x.Name == "TitleLabel");
                if (wmsLabel != null)
                {
                    wmsLabel.AdditionalInfo = binding ? $"{string.Format(Common.Resources.General.AdditionalInfo, prop)}" : $"{prop}";
                }

                baseEdit.Loaded -= BaseEdit_Loaded;
            }
            else
            {
                baseEdit.Loaded += BaseEdit_Loaded;
            }
        }

        #endregion
    }
}
