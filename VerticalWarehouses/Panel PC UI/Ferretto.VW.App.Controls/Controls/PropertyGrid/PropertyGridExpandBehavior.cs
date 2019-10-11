using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using DevExpress.Xpf.PropertyGrid;

namespace Ferretto.VW.App.Controls
{
    public static class PropertyGridRowControlAttachedProperties
    {
        #region Fields

        public static readonly DependencyProperty IsIsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool),
                                                             typeof(PropertyGridRowControlAttachedProperties),
                                                             new UIPropertyMetadata(false, OnIsEnabledChanged));

        #endregion

        #region Methods

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsIsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsIsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is RowControl rowControl))
            {
                return;
            }

            var isEnabledMode = GetIsEnabled(d);
            var behaviors = Interaction.GetBehaviors(d);
            var behavior = behaviors.SingleOrDefault(x => x is PropertyGridExpandBehavior);

            if (behavior != null && !isEnabledMode)
            {
                behaviors.Remove(behavior);
            }
            else if (behavior == null && isEnabledMode)
            {
                behavior = new PropertyGridExpandBehavior();
                behaviors.Add(behavior);
            }
        }

        #endregion
    }

    public class PropertyGridExpandBehavior : Behavior<RowControl>
    {
        #region Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.PreviewMouseLeftButtonDown += Row_MouseDoubleClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.PreviewMouseLeftButtonDown -= Row_MouseDoubleClick;
        }

        private static void Row_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var rowData = (RowData)element.DataContext;
            rowData.IsExpanded = !rowData.IsExpanded;
        }

        #endregion
    }
}
