using System.Linq;
using System.Windows;
using DevExpress.Mvvm.UI;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Controls
{
    public class AdditionalInfo : DependencyObject
    {
        #region Fields

        public static readonly DependencyProperty OriginalTextProperty = DependencyProperty.RegisterAttached(
            "OriginalText", typeof(string), typeof(AdditionalInfo));

        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text", typeof(string), typeof(AdditionalInfo), new UIPropertyMetadata(OnTextChanged));

        #endregion

        #region Properties

        public string NewValue { get; set; }

        #endregion

        #region Methods

        public static string GetText(DependencyObject element) => (string)element?.GetValue(TextProperty);

        public static void SetText(DependencyObject element, string value) => element?.SetValue(TextProperty, value);

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetAdditionalInfoInTitleLabel(d, true);
        }

        private static void SetAdditionalInfoInTitleLabel(DependencyObject element, bool binding)
        {
            if (element == null)
            {
                return;
            }

            var labelText = (string)element.GetValue(TextProperty);
            var originalText = (string)element.GetValue(OriginalTextProperty);

            if (element is DevExpress.Xpf.Grid.GridColumn gridColumn)
            {
                if (originalText == null)
                {
                    originalText = (gridColumn.HeaderCaption ?? gridColumn.Header)?.ToString();
                    element.SetValue(OriginalTextProperty, originalText);
                }

                var additionalInfoText = string.Format(General.AdditionalInfo, labelText);

                gridColumn.Header = string.Format(
                    General.TitleWithAdditionalInfo,
                    originalText,
                    additionalInfoText);
            }
            else
            {
                var wmsLabel = LayoutTreeHelper
                    .GetVisualChildren(element)
                    .OfType<WmsLabel>()
                    .FirstOrDefault(x => x.Name == "TitleLabel");

                if (wmsLabel != null)
                {
                    wmsLabel.AdditionalInfo = (binding && !string.IsNullOrEmpty(labelText)) ?
                        $"{string.Format(General.AdditionalInfo, labelText)}" :
                        $"{labelText}";
                }
            }
        }

        #endregion
    }
}
