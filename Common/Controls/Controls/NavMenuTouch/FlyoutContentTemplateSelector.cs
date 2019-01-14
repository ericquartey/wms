using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public class FlyoutContentTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                var template = (DataTemplate)Application.Current.Resources["FlyoutCollectionTemplate"];
                return template;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
