using System;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Navigation;

namespace Ferretto.Common.Controls
{
  public class CustomMenuTileFlyoutPosition : DependencyObject
  {
    #region Dependency properties
    public static bool GetEnabled(DependencyObject obj)
    {
      return (bool)obj.GetValue(EnabledProperty);
    }

    public static void SetEnabled(DependencyObject obj, bool value)
    {
      obj.SetValue(EnabledProperty, value);
    }

    #pragma warning disable IDE1006 // Naming Styles
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
    #pragma warning restore IDE1006 // Naming Styles
        "Enabled",
        typeof(bool),
        typeof(CustomMenuTileFlyoutPosition),
        new PropertyMetadata(OnSetEnabledChanged));

    private static void OnSetEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (dependencyObject is FrameworkElement element)
      {
        element.Loaded += Element_Loaded;
        element.PreviewMouseDown += PreviewMouseDown;
      }
    }
    #endregion

    #region Handled events
    private static void Element_Loaded(object sender, RoutedEventArgs e)
    {
      if (!(sender is TileBar tileBar))
      {
        return;
      }
      if (LayoutTreeHelper.GetVisualParents(tileBar as DependencyObject)
                          .OfType<TileBarContentControl>()
                          .FirstOrDefault() != null)
      {
        var flyout = (DevExpress.Xpf.Editors.Flyout.Native.FlyoutBase)tileBar.GetValue(DevExpress.Xpf.Editors.Flyout.Native.FlyoutBase.FlyoutProperty);
        flyout.Padding = new Thickness(0, -tileBar.ActualHeight, 0, 0);
        flyout.UpdateLayout();
      }
    }

    private static void PreviewMouseDown(object sender, RoutedEventArgs e)
    {
      if (!(sender is TileBar tileBar))
      {
        return;
      }

      var flyout = (DevExpress.Xpf.Editors.Flyout.Native.FlyoutBase)tileBar.GetValue(DevExpress.Xpf.Editors.Flyout.Native.FlyoutBase.FlyoutProperty);
      if (flyout != null)
      {
        ((FrameworkElement)(flyout.Content)).Opacity = 0;
        tileBar.IsVisibleChanged -= Flyout_IsVisibleChanged;
        tileBar.IsVisibleChanged += Flyout_IsVisibleChanged;
      }

      var canClose = true;
      if (e.Source is TileBarItem tileBarItem)
      {
        if (tileBarItem.DataContext is IMenuItemViewModel dt &&
            dt.HasChildren == false)                 
        {
          tileBarItem.Command?.Execute(null);
          e.Handled = true;
        }
        else
        {
          canClose = false;
        }
      }

      if (canClose)
      {
        if (flyout != null)
        {
          flyout.Closed -= Flyout_Closed;
          flyout.Closed += Flyout_Closed;
          flyout.IsOpen = false;
        }        
      }
    }

    private static void Flyout_Closed(object sender, EventArgs e)
    {
      CloseParentFlyout(sender as UIElement);
    }

    private static void Flyout_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is TileBar tileBar))
      {
        return;
      }
      var flyout = (DevExpress.Xpf.Editors.Flyout.Native.FlyoutBase)tileBar.GetValue(DevExpress.Xpf.Editors.Flyout.Native.FlyoutBase.FlyoutProperty);
      if (flyout != null)
      {
        ((FrameworkElement)(flyout.Content)).Opacity = 1;
      }
    }
    #endregion

    #region Methods
    private static void CloseParentFlyout(UIElement element)
    {
      if (element == null)
      {
        return;
      }
      var bar = LayoutHelper.FindParentObject<TileBar>(element);
      var parents = LayoutTreeHelper.GetVisualParents(bar);
      var flyoutContainerControl = parents.OfType<DevExpress.Xpf.Editors.Flyout.FlyoutContainerControl>();
      foreach (DevExpress.Xpf.Editors.Flyout.FlyoutContainerControl flyoutC in flyoutContainerControl)
      {
        var fly = flyoutC.TemplatedParent as DevExpress.Xpf.Controls.Primitives.FlyoutAdornerControl;
        fly.IsOpen = false;
      }
    }
    #endregion
  }
}
