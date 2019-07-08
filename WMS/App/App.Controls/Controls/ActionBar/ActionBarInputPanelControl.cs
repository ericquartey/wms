using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Flyout;
using DevExpress.Xpf.Editors.Flyout.Native;
using Ferretto.WMS.App.Controls;

namespace Ferretto.WMS.App.Controls
{
    public class ActionBarInputPanelControl : FlyoutControl
    {
        #region Fields

        public static readonly DependencyProperty AttachedActionBarProperty = DependencyProperty.Register(
                                                 nameof(AttachedActionBar),
                                                 typeof(ToolBarControl),
                                                 typeof(ActionBarInputPanelControl));

        public static readonly DependencyProperty AttachedBarItemProperty = DependencyProperty.Register(
                                                 nameof(AttachedBarItem),
                                                 typeof(string),
                                                 typeof(ActionBarInputPanelControl),
                                                 new FrameworkPropertyMetadata(default(string), null));

        public static readonly DependencyProperty FocusedStartProperty = DependencyProperty.Register(
                                                        nameof(FocusedStart),
                                        typeof(string),
                                        typeof(ActionBarInputPanelControl),
                                        new FrameworkPropertyMetadata(default(string), null));

        #endregion

        #region Constructors

        public ActionBarInputPanelControl()
        {
            var component = this;
            DependencyPropertyDescriptor.FromProperty(IsOpenProperty, typeof(FlyoutBase)).AddValueChanged(component, handler: this.FlyoutControl_IsOpenChanged);
        }

        #endregion

        #region Properties

        public ToolBarControl AttachedActionBar
        {
            get => (ToolBarControl)this.GetValue(AttachedActionBarProperty);
            set => this.SetValue(AttachedActionBarProperty, value);
        }

        public string AttachedBarItem
        {
            get => (string)this.GetValue(AttachedBarItemProperty);
            set => this.SetValue(AttachedBarItemProperty, value);
        }

        public string FocusedStart
        {
            get => (string)this.GetValue(FocusedStartProperty);
            set => this.SetValue(FocusedStartProperty, value);
        }

        #endregion

        #region Methods

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);
            this.AttachEvent();
        }

        private void AttachedActionBarItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.PlacementTarget = e.Link.LinkControl as FrameworkElement;
            this.StaysOpen = false;
            this.IsOpen = true;
        }

        private void AttachEvent()
        {
            if (this.AttachedActionBar?.Items is ObservableCollection<IBarItem> barItems)
            {
                var itemFound = barItems.FirstOrDefault(i => ((BarButtonItem)i).Tag != null && ((BarButtonItem)i).Tag.Equals(this.AttachedBarItem));
                if (itemFound is BarButtonItem buttonItem)
                {
                    buttonItem.ItemClick += this.AttachedActionBarItem_ItemClick;
                }
            }

            var parent = LayoutTreeHelper.GetVisualParents(this).OfType<WmsView>().FirstOrDefault();
            if (parent is WmsView wmsView)
            {
                wmsView.Unloaded += this.Parent_Unloaded;
            }
        }

        private void FlyoutControl_IsOpenChanged(object sender, EventArgs e)
        {
            var flyoutControl = sender as FlyoutControl;
            if (flyoutControl.IsOpen)
            {
                flyoutControl.Dispatcher.BeginInvoke(
                        DispatcherPriority.ApplicationIdle,
                        this.SetFocus(flyoutControl));
            }
        }

        private void Parent_Unloaded(object sender, RoutedEventArgs e)
        {
            DependencyPropertyDescriptor.FromProperty(FlyoutBase.IsOpenProperty, typeof(FlyoutBase)).RemoveValueChanged(this, this.FlyoutControl_IsOpenChanged);
        }

        private Action SetFocus(FlyoutBase flyout)
        {
            return () =>
            {
                if (LayoutTreeHelper.GetVisualChildren(flyout.ChildContainer).OfType<BaseEdit>().FirstOrDefault(ui => ui.Name == this.FocusedStart) is BaseEdit elementToFocus)
                {
                    elementToFocus.Focus();
                }
            };
        }

        #endregion
    }
}
