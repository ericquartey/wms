﻿using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Docking.Base;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Prism.Regions;

namespace Ferretto.WMS.App.Controls
{
    public class WmsMainDockLayoutManager : DockLayoutManager
    {
        #region Fields

        public static readonly DependencyProperty ShowBusyOnStartUpProperty = DependencyProperty.Register(nameof(ShowBusyOnStartUp), typeof(bool), typeof(WmsMainDockLayoutManager));

        public static readonly DependencyProperty StartModuleNameProperty = DependencyProperty.Register(nameof(StartModuleName), typeof(string), typeof(WmsMainDockLayoutManager));

        public static readonly DependencyProperty StartViewNameProperty = DependencyProperty.Register(nameof(StartViewName), typeof(string), typeof(WmsMainDockLayoutManager));

        private readonly INavigationService navigationService;

        private LoadingDecorator busyIndicator;

        private bool isControlPressed;

        #endregion

        #region Constructors

        public WmsMainDockLayoutManager()
        {
            Current = this;
            this.DataContext = null;
            this.navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

            this.Loaded += this.WmsMainDockLayoutManager_Loaded;
            this.DockItemClosing += WmsMainDockLayoutManager_DockItemClosing;
            this.DockOperationCompleted += this.WmsMainDockLayoutManager_DockOperationCompleted;
            this.ClosedPanelsBarVisibility = ClosedPanelsBarVisibility.Never;
        }

        #endregion

        #region Properties

        public static WmsMainDockLayoutManager Current { get; private set; }

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

        #endregion

        #region Methods

        public static INavigableView GetActiveView()
        {
            if (!(WmsMainDockLayoutManager.Current.DockController.ActiveItem is LayoutPanel activePanel))
            {
                return null;
            }

            return activePanel.Content as INavigableView;
        }

        public static void IsBusy(bool busy)
        {
            if (WmsMainDockLayoutManager.Current.busyIndicator is LoadingDecorator busyIndicator)
            {
                busyIndicator.IsSplashScreenShown = busy;
            }
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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.Parent is LoadingDecorator busyIndicator)
            {
                this.busyIndicator = busyIndicator;
                this.busyIndicator.IsSplashScreenShown = this.ShowBusyOnStartUp;
            }
        }

        public void RegisterView(string regionName, string title, INavigableView viewToActivate, IRegionManager regionManager)
        {
            DocumentGroup activeGroup = null;

            var newLayoutPanel = new LayoutPanel();
            newLayoutPanel.Caption = title;
            RegionManager.SetRegionManager(newLayoutPanel, regionManager);
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

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var inputService = ServiceLocator.Current.GetInstance<IInputService>();
            inputService.BeginMouseNotify(this, this.OnMouseDown);
            inputService.BeginShortKeyNotify(this, (shortKey) => this.isControlPressed = (shortKey.ShortKey.ModifierKeyFirst == ModifierKeys.Control));
        }

        private static void WmsMainDockLayoutManager_DockItemClosing(object sender, ItemCancelEventArgs e)
        {
            if (!(((DevExpress.Xpf.Docking.ContentItem)e.Item).Content is WmsView vmsView))
            {
                return;
            }

            if (vmsView.CanDisappear())
            {
                vmsView.Disappear();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void NewLayoutPanel_Loaded(object sender, RoutedEventArgs e)
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
            this.isControlPressed = (Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control;
        }

        private void WmsMainDockLayoutManager_DockOperationCompleted(object sender, DockOperationCompletedEventArgs e)
        {
            var item = e.Item;
            if (item.Parent == null)
            {
                // Is Closed
                return;
            }

            if (item is DevExpress.Xpf.Docking.LayoutGroup)
            {
                return;
            }

            if (item.Parent.GetType() == typeof(DevExpress.Xpf.Docking.LayoutGroup))
            {
                var parentLayoutGroup = item.Parent;

                var docLayoutGroup = this.CreateLayoutGroup();
                var docGroup = this.CreateDocumentGroup();
                docGroup.DestroyOnClosingChildren = true;
                docLayoutGroup.Add(docGroup);
                if (parentLayoutGroup.Items.IndexOf(item) == 0)
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

        private void WmsMainDockLayoutManager_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.StartModuleName) == false &&
                string.IsNullOrEmpty(this.StartViewName) == false &&
                this.navigationService.IsUnitTest == false)
            {
                var notificationService = ServiceLocator.Current.GetInstance<INotificationService>();
                notificationService.CheckForDataErrorConnection();
                this.navigationService.Appear(this.StartModuleName, this.StartViewName);
            }
        }

        #endregion
    }
}
