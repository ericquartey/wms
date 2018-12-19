using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Docking.Base;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;
using Prism.Regions;

namespace Ferretto.Common.Controls
{
    public class WmsMainDockLayoutManager : DockLayoutManager
    {
        #region Fields

        private readonly IInputService inputService;
        private readonly INavigationService navigationService;
        private bool isControlPressed = false;
        private LoadingDecorator busyIndicator;

        #endregion Fields

        public static readonly DependencyProperty StartModuleNameProperty = DependencyProperty.Register(nameof(StartModuleName), typeof(string), typeof(WmsMainDockLayoutManager));
        public static readonly DependencyProperty StartViewNameProperty = DependencyProperty.Register(nameof(StartViewName), typeof(string), typeof(WmsMainDockLayoutManager));
        public static readonly DependencyProperty ShowBusyOnStartUpProperty = DependencyProperty.Register(nameof(ShowBusyOnStartUp), typeof(bool), typeof(WmsMainDockLayoutManager));
        public bool ShowBusyOnStartUp
        {
            get => (bool)this.GetValue(ShowBusyOnStartUpProperty);
            set => this.SetValue(ShowBusyOnStartUpProperty, value);
        }

        public string StartModuleName
        {
            get => (string)this.GetValue(StartModuleNameProperty);
            set => this.SetValue(StartModuleNameProperty, value);
        }

        public string StartViewName
        {
            get => (string)this.GetValue(StartViewNameProperty);
            set => this.SetValue(StartViewNameProperty, value);
        }
        #region Constructors

        public WmsMainDockLayoutManager()
        {
            Current = this;
            this.DataContext = null;
            this.navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            this.inputService = ServiceLocator.Current.GetInstance<IInputService>();
            this.Loaded += this.WmsMainDockLayoutManager_Loaded;
            this.DockItemClosing += this.WmsMainDockLayoutManager_DockItemClosing;
            this.DockOperationCompleted += this.WmsMainDockLayoutManager_DockOperationCompleted;
            this.ClosedPanelsBarVisibility = ClosedPanelsBarVisibility.Never;
            this.inputService.BeginMouseNotify(this, this.OnMouseDown);
            this.inputService.BeginShortKeyNotify(this, (shortKey) => this.isControlPressed = (shortKey.ShortKey.ModifierKeyFirst == ModifierKeys.Control));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.Parent is LoadingDecorator busyIndicator)
            {
                this.busyIndicator = busyIndicator;
                this.busyIndicator.IsSplashScreenShown = this.ShowBusyOnStartUp;
            }
        }

        private void WmsMainDockLayoutManager_Loaded(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if ((string.IsNullOrEmpty(this.StartModuleName) &&
                string.IsNullOrEmpty(this.StartViewName)) == false)
            {
                this.navigationService.Appear(this.StartModuleName, this.StartViewName);
            }
        }

        #endregion Constructors

        #region Properties

        public static WmsMainDockLayoutManager Current
        { get; private set; }

        #endregion Properties

        #region Methods

        public static INavigableView GetActiveView()
        {
            if (!(WmsMainDockLayoutManager.Current.DockController.ActiveItem is LayoutPanel activePanel))
            {
                return null;
            }
            return activePanel.Content as INavigableView;
        }

        public void ActivateView(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
            {
                return;
            }

            var layoutPanel = this.GetItems().Where((item) => item is LayoutPanel && (((LayoutPanel)item).Content is WmsView))
                                             .FirstOrDefault(v => ((WmsView)((LayoutPanel)v).Content).MapId == mapId);
            if (layoutPanel != null)
            {
                layoutPanel.IsActive = true;
            }
        }

        public void RegisterView(string regionName, string title)
        {
            DocumentGroup activeGroup = null;

            var newLayoutPanel = new LayoutPanel();
            newLayoutPanel.Caption = title;
            RegionManager.SetRegionName(newLayoutPanel, regionName);

            if (this.ActiveDockItem != null && (this.ActiveDockItem.Parent is DocumentGroup))
            {
                activeGroup = this.ActiveDockItem.Parent as DocumentGroup;
            }

            if (activeGroup == null && (this.FindName("MainDocumentGroup") is DocumentGroup))
            {
                activeGroup = this.FindName("MainDocumentGroup") as DocumentGroup;
            }

            if (activeGroup == null)
            {
                throw new System.InvalidOperationException(Errors.CannotRetrieveDocumentGroupFromLayoutManager);
            }

            newLayoutPanel.AllowFloat = false;
            newLayoutPanel.AllowHide = false;
            newLayoutPanel.IsActive = true;
            newLayoutPanel.Loaded += this.NewLayoutPanel_Loaded;
            if (this.isControlPressed == false)
            {
                if (!(this.DockController.ActiveItem is LayoutPanel activePanel))
                {
                    // All Views closed
                    activeGroup.Add(newLayoutPanel);
                    return;
                }
                var layoutGroup = activePanel.Parent;
                var lastActivePosition = layoutGroup.Items.IndexOf(activePanel);
                layoutGroup.Items.Insert(lastActivePosition, newLayoutPanel);
                this.DockController.RemovePanel(activePanel);

                var vmsView = activePanel.Content;
                if (vmsView is INavigableView view)
                {
                    view.Disappear();
                }
            }
            else
            {
                this.isControlPressed = false;
                activeGroup.Add(newLayoutPanel);
            }
        }

        private void NewLayoutPanel_Loaded(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.busyIndicator != null &&
                this.ShowBusyOnStartUp)
            {
                this.ShowBusyOnStartUp = false;
                WmsMainDockLayoutManager.IsBusy(false);
            }
            if (sender is LayoutPanel layoutPanel)
            {
                layoutPanel.Loaded -= this.NewLayoutPanel_Loaded;
            }
        }

        private void OnMouseDown(MouseDownInfo mouseDownInfo)
        {
            this.isControlPressed = ((Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control);
        }

        private void WmsMainDockLayoutManager_DockItemClosing(System.Object sender, DevExpress.Xpf.Docking.Base.ItemCancelEventArgs e)
        {
            if (!(((DevExpress.Xpf.Docking.ContentItem)e.Item).Content is INavigableView vmsView))
            {
                return;
            }

            ((INavigableView)vmsView).Disappear();
        }

        private void WmsMainDockLayoutManager_DockOperationCompleted(System.Object sender, DevExpress.Xpf.Docking.Base.DockOperationCompletedEventArgs e)
        {
            var item = e.Item;
            var source = e.Source;
            if (item.Parent == null)
            {   // Is Closed
                return;
            }
            if (item is DevExpress.Xpf.Docking.LayoutGroup)
            {
                return;
            }
            if (item.Parent.GetType() == typeof(DevExpress.Xpf.Docking.LayoutGroup))
            {
                var parentLayoutGroup = item.Parent as LayoutGroup;

                var docLayoutGroup = this.CreateLayoutGroup();
                var docGroup = this.CreateDocumentGroup();
                docGroup.DestroyOnClosingChildren = true;
                docLayoutGroup.Add(docGroup);
                if ((parentLayoutGroup.Items.IndexOf(item)) == 0)
                {
                    parentLayoutGroup.Items.Insert(0, docGroup);
                }
                else
                {
                    parentLayoutGroup.Items.Add(docGroup);
                }

                this.LayoutController.Move(item, docGroup, DevExpress.Xpf.Layout.Core.MoveType.InsideGroup);
            }
        }

        public static void IsBusy(bool isBusy)
        {
            if (WmsMainDockLayoutManager.Current.busyIndicator is LoadingDecorator busyIndicator)
            {
                busyIndicator.IsSplashScreenShown = isBusy;
            }
        }

        #endregion Methods
    }
}
