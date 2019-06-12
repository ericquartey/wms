using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Editors.Flyout;

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
                var itemFound = barItems.FirstOrDefault(i => ((BarButtonItem)i).Tag.Equals(this.AttachedBarItem));
                if (itemFound is BarButtonItem buttonItem)
                {
                    buttonItem.ItemClick += this.AttachedActionBarItem_ItemClick;
                }
            }
        }

        #endregion
    }
}
