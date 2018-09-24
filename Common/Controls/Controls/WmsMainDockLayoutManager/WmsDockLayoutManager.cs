using System.Linq;
using System.Windows.Controls;
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

        #endregion Fields

        #region Constructors

        public WmsMainDockLayoutManager()
        {
            Current = this;
            this.navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            this.DockItemClosing += this.WmsMainDockLayoutManager_DockItemClosing;
            this.DockOperationCompleted += this.WmsMainDockLayoutManager_DockOperationCompleted;
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
            var layoutPanel = new LayoutPanel();
            layoutPanel.Caption = title;
            RegionManager.SetRegionName(layoutPanel, regionName);
            if (!(this.FindName("MainDocumentGroup") is DocumentGroup mainGroup))
            {
                throw new System.InvalidOperationException(Errors.CannotRetrieveDocumentGroupFromLayoutManager);
            }
            layoutPanel.AllowFloat = false;
            layoutPanel.AllowHide = false;
            mainGroup.Add(layoutPanel);
            layoutPanel.IsActive = true;
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
