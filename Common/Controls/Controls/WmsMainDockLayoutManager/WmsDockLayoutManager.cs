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
        #region fields

        private readonly INavigationService navigationService;

        #endregion

        #region Constructors

        public WmsMainDockLayoutManager()
        {
            Current = this;
            this.navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            this.DockItemClosing += this.WmsMainDockLayoutManager_DockItemClosing;
        }

        #endregion Constructors

        #region Properties

        public static WmsMainDockLayoutManager Current { get; private set; }

        #endregion Properties

        #region Events

        private void WmsMainDockLayoutManager_DockItemClosing(System.Object sender, DevExpress.Xpf.Docking.Base.ItemCancelEventArgs e)
        {
            if (!(((DevExpress.Xpf.Docking.ContentItem)e.Item).Content is INavigableView vmsView))
            {
                return;
            }
            var viewModel = ((UserControl)vmsView).DataContext;
            this.navigationService.Disappear(viewModel as INavigableViewModel);
        }

        #endregion

        #region Methods

        public void RegisterView(string regionName, string title)
        {
            var layoutPanel = new LayoutPanel();
            layoutPanel.Caption = title;
            RegionManager.SetRegionName(layoutPanel, regionName);
            if (!( this.FindName("MainDocumentGroup") is DocumentGroup mainGroup ))
            {
                throw new System.InvalidOperationException(Errors.CannotRetrieveDocumentGroupFromLayoutManager);
            }

            mainGroup.Add(layoutPanel);
        }

        #endregion Methods
    }
}
