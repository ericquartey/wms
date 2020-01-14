using System.Windows;
using System.Windows.Media;

namespace Ferretto.VW.App.Controls
{
    public static class VirtualTreeHelperExtensions
    {
        public static T FindAncestor<T>(this DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);

            if (parent == null)
            {
                return null;
            }
            var parentT = parent as T;
            return parentT ?? FindAncestor<T>(parent);
        }
    }
}
