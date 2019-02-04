using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ferretto.Common.Controls
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

        #region Constructors

        public AccordionItem()
        {
            this.IsExpanded = false;
        }

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
