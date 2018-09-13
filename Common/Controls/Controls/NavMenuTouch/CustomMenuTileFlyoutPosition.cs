using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Navigation;
using DevExpress.Xpf.Navigation.Internal;

namespace Ferretto.Common.Controls
{
    public class CustomMenuTileFlyoutPosition : DependencyObject
    {
        #region Fields

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled",
            typeof(bool),
            typeof(CustomMenuTileFlyoutPosition),
            new PropertyMetadata(OnSetEnabledChanged));

        #endregion Fields

        #region Methods

        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool) obj?.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj?.SetValue(EnabledProperty, value);
        }

        private static void CloseParentFlyout(UIElement element)
        {
            if (element == null)
            {
                return;
            }

            var bar = LayoutHelper.FindParentObject<TileBar>(element);
            var parents = LayoutTreeHelper.GetVisualParents(bar);
            var flyoutContainerControl = parents.OfType<DevExpress.Xpf.Editors.Flyout.FlyoutContainerControl>();
            foreach (var flyoutC in flyoutContainerControl)
            {
                var fly = flyoutC.TemplatedParent as DevExpress.Xpf.Controls.Primitives.FlyoutAdornerControl;
                fly.IsOpen = false;
            }
        }

        private static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            if (!( sender is TileBar tileBar ))
            {
                return;
            }

            if (LayoutTreeHelper.GetVisualParents(tileBar as DependencyObject)
                    .OfType<TileBarContentControl>()
                    .FirstOrDefault() != null)
            {
                var flyout =
                    (DevExpress.Xpf.Editors.Flyout.Native.FlyoutBase) tileBar.GetValue(DevExpress.Xpf.Editors.Flyout
                        .Native.FlyoutBase.FlyoutProperty);
                // Change position of child tilebar
                flyout.Padding = new Thickness(0, -( tileBar.ActualHeight - 10 ), 0, 0);
                flyout.UpdateLayout();
            }
        }

        private static void Flyout_Closed(object sender, EventArgs e)
        {
            CloseParentFlyout(sender as UIElement);
        }

        private static void Flyout_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!( sender is TileBar tileBar ))
            {
                return;
            }

            var flyout =
                (DevExpress.Xpf.Editors.Flyout.Native.FlyoutBase) tileBar.GetValue(DevExpress.Xpf.Editors.Flyout.Native
                    .FlyoutBase.FlyoutProperty);
            if (flyout != null)
            {
                ( (FrameworkElement) ( flyout.Content ) ).Opacity = 1;
            }
        }

        private static void GridParent_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!( sender is Grid grid ))
            {
                return;
            }

            var tileBar = LayoutTreeHelper.GetVisualChildren(grid as DependencyObject)
                .OfType<TileBar>()
                .FirstOrDefault();
            if (tileBar != null)
            {
                if (IsClickOnScroll(e.OriginalSource))
                {
                    return;
                }

                var flyout =
                    (DevExpress.Xpf.Editors.Flyout.Native.FlyoutBase) tileBar.GetValue(DevExpress.Xpf.Editors.Flyout
                        .Native.FlyoutBase.FlyoutProperty);
                if (flyout != null)
                {
                    tileBar.IsVisibleChanged -= Flyout_IsVisibleChanged;
                    tileBar.IsVisibleChanged += Flyout_IsVisibleChanged;
                    if (( e.Source is TileNavPaneContentControl ) == false)
                    {
                        e.Handled = true;
                        flyout.IsOpen = false;
                    }
                }
            }
        }

        private static bool IsClickOnScroll(object originalSource)
        {
            return ( LayoutTreeHelper.GetVisualParents(originalSource as UIElement)
                         .OfType<DevExpress.Xpf.Controls.Primitives.ScrollableControlButton>()
                         .FirstOrDefault() != null );
        }

        private static void OnSetEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is FrameworkElement element)
            {
                element.Loaded += Element_Loaded;
                element.PreviewMouseDown += PreviewMouseDown;
                if (element.Parent != null)
                {
                    // Is not first level /flyout
                    var grid = LayoutTreeHelper.GetVisualParents(dependencyObject)
                        .OfType<Grid>().FirstOrDefault(x => x.Name == "PART_RenderGrid");
                    if (grid != null)
                    {
                        // Consider all flyout content
                        grid.PreviewMouseDown += GridParent_PreviewMouseDown;
                    }
                }
            }
        }

        private static void PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            if (!( sender is TileBar tileBar ))
            {
                return;
            }

            var flyout =
                (DevExpress.Xpf.Editors.Flyout.Native.FlyoutBase) tileBar.GetValue(DevExpress.Xpf.Editors.Flyout.Native
                    .FlyoutBase.FlyoutProperty);
            if (flyout != null)
            {
                tileBar.IsVisibleChanged -= Flyout_IsVisibleChanged;
                tileBar.IsVisibleChanged += Flyout_IsVisibleChanged;
            }

            var canClose = true;
            if (e.Source is TileBarItem tileBarItem)
            {
                if (flyout != null)
                {
                    ( (FrameworkElement) ( flyout.Content ) ).Opacity = 0;
                }

                var dt = tileBarItem.DataContext as IMenuItemViewModel;
                if (dt != null && dt.HasChildren == false)
                {
                    tileBarItem.Command?.Execute(null);
                    e.Handled = true;
                }
                else
                {
                    canClose = false;
                }
            }
            else
            {
                if (IsClickOnScroll(e.OriginalSource))
                {
                    canClose = false;
                }
            }

            if (canClose && flyout != null)
            {
                flyout.Closed -= Flyout_Closed;
                flyout.Closed += Flyout_Closed;
                flyout.IsOpen = false;
            }
        }

        #endregion Methods
    }
}
