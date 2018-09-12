using DevExpress.Xpf.Docking;
using Ferretto.Common.Resources;
using Prism.Regions;

namespace Ferretto.Common.Controls
{
    public class WmsMainDockLayoutManager : DockLayoutManager
    {
        #region Constructors

        public WmsMainDockLayoutManager()
        {
            Current = this;
        }

        #endregion Constructors

        #region Properties

        public static WmsMainDockLayoutManager Current { get; private set; }

        #endregion Properties

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
