using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Editors;

namespace Ferretto.Common.Controls
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

        public static string GetText(DependencyObject element) => (string)element.GetValue(TextProperty);

        public static void SetText(DependencyObject element, string value) => element.SetValue(TextProperty, value);

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
            if (d is IBaseEdit baseEdit)
            {
                var prop = baseEdit.GetValue(AdditionalInfo.TextProperty);

                var y = LayoutTreeHelper.GetVisualParents(baseEdit.EditCore as DependencyObject)
                 .OfType<Grid>()
                 .FirstOrDefault(x => x.Name == "TextEditGrid");
                if (y != null)
                {
                    var wmsLabel = y.Children.OfType<WmsLabel>().FirstOrDefault(x => x.Name == "TitleLabel");
                    if (wmsLabel.Title != null &&
                        wmsLabel.OriginalTitle == null)
                    {
                        wmsLabel.OriginalTitle = wmsLabel.Title;
                    }

                    if (binding)
                    {
                        wmsLabel.Title = $"{wmsLabel.OriginalTitle} {String.Format(Resources.General.AdditionalInfo, prop)}";
                    }
                    else
                    {
                        wmsLabel.Title = $"{wmsLabel.OriginalTitle} {prop}";
                    }

                    baseEdit.Loaded -= BaseEdit_Loaded;
                }
                else
                {
                    baseEdit.Loaded += BaseEdit_Loaded;
                }
            }
        }

        #endregion
    }
}
