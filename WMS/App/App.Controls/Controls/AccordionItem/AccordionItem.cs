using System.Windows;

namespace Ferretto.WMS.App.Controls
{
    /// <summary>
    /// Interaction logic for Accordion.xaml
    /// </summary>
    public class AccordionItem : DevExpress.Xpf.Accordion.AccordionItem
    {
        #region Fields

        public static readonly DependencyProperty IsExpandButtonVisibleProperty = DependencyProperty.Register(
           nameof(IsExpandButtonVisible), typeof(bool), typeof(AccordionItem), new UIPropertyMetadata(true));

        public static readonly DependencyProperty IsToggleVisibleProperty = DependencyProperty.Register(
                   nameof(IsToggleVisible), typeof(bool), typeof(AccordionItem), new UIPropertyMetadata(false));

        public static readonly DependencyProperty TitleHeaderProperty = DependencyProperty.Register(
                           nameof(TitleHeader), typeof(string), typeof(AccordionItem), new UIPropertyMetadata(string.Empty));

        #endregion

        #region Properties

        public bool IsExpandButtonVisible
        {
            get => (bool)this.GetValue(IsExpandButtonVisibleProperty);
            set => this.SetValue(IsExpandButtonVisibleProperty, value);
        }

        public bool IsToggleVisible
        {
            get => (bool)this.GetValue(IsToggleVisibleProperty);
            set => this.SetValue(IsToggleVisibleProperty, value);
        }

        public string TitleHeader
        {
            get => (string)this.GetValue(TitleHeaderProperty);
            set => this.SetValue(TitleHeaderProperty, value);
        }

        #endregion
    }
}
