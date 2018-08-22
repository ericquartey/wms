using DevExpress.Xpf.Docking;
using Prism.Regions;

namespace Ferretto.Common.Controls
{
  public class WmsMainDockLayoutManager : DockLayoutManager
  {
    #region Static Property
    public static WmsMainDockLayoutManager Current
    {
      get;
      private set;
    }
    #endregion

    #region Ctor
    public WmsMainDockLayoutManager()
    {
      Current = this;
    }
    #endregion

    #region Methods
    public void RegisterView(string regionName, string title)
    {
      var layoutPanel = new LayoutPanel();
      layoutPanel.Caption = title;
      RegionManager.SetRegionName(layoutPanel, regionName);
      if (!(this.FindName("MainDocumentGroup") is DocumentGroup mainGroup))
      {
        throw new System.InvalidOperationException("Error retrieving document group from the Main Layout Manager");
      }
      mainGroup.Add(layoutPanel);
    }
    #endregion
  }
}
