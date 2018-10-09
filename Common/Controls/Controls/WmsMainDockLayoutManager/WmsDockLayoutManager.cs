using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Docking;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;
using Prism.Regions;

namespace Ferretto.Common.Controls
{
    public class WmsMainDockLayoutManager : DockLayoutManager
    {
        #region Fields

        private readonly INavigationService navigationService;
        private bool isControlPressed = true;

        #endregion Fields

        #region Constructors

        public WmsMainDockLayoutManager()
        {
            Current = this;
            this.navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            this.DockItemClosing += this.WmsMainDockLayoutManager_DockItemClosing;
            this.DockOperationCompleted += this.WmsMainDockLayoutManager_DockOperationCompleted;
            Application.Current.MainWindow.PreviewKeyDown += this.DockHost_PreviewKeyDown;
            Application.Current.MainWindow.LostMouseCapture += this.MainWindow_LostMouseCapture;
        }

        #endregion Constructors

        #region Properties

        public static WmsMainDockLayoutManager Current { get; private set; }

        #endregion Properties

        #region Methods

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

            if (this.isControlPressed == false)
            {
                var activePanel = this.DockController.ActiveItem as LayoutPanel;
                var layoutGroup = activePanel.Parent;
                var lastActivePosition = layoutGroup.Items.IndexOf(activePanel);
                this.DockController.RemovePanel(activePanel);
                layoutGroup.Items.Insert(lastActivePosition, newLayoutPanel);
            }
            else
            {
                activeGroup.Add(newLayoutPanel);
            }
        }

        private void DockHost_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                this.isControlPressed = true;
            }
        }

        private void MainWindow_LostMouseCapture(Object sender, MouseEventArgs e)
        {
            this.isControlPressed = false;
        }

        private void MainWindow_MouseUp(Object sender, MouseButtonEventArgs e)
        {
            this.isControlPressed = false;
        }

        private void WmsMainDockLayoutManager_DockItemClosing(System.Object sender, DevExpress.Xpf.Docking.Base.ItemCancelEventArgs e)
        {
            if (!(((DevExpress.Xpf.Docking.ContentItem)e.Item).Content is INavigableView vmsView))
            {
                return;
            }
            var viewModel = ((UserControl)vmsView).DataContext;
            this.navigationService.Disappear(viewModel as INavigableViewModel);
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

        #endregion Methods
    }
}
