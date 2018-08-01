using DevExpress.Xpf.Docking;
using Prism.Regions;

namespace Ferretto.Common.Controls
{
  public class WMSMainDockLayoutManager : DockLayoutManager
  {
    #region Static Property
    public static WMSMainDockLayoutManager Current
    {
      get;
      private set;
    }
    #endregion

    #region Ctor
    public WMSMainDockLayoutManager()
    {
      Current = this;
    }
    #endregion

    #region Methods
    public void RegisterView(string regionName, string title)
    {
      LayoutPanel layoutPanel = new LayoutPanel();
      layoutPanel.Caption = title;
      RegionManager.SetRegionName(layoutPanel, regionName);
      DocumentGroup mainGroup = this.FindName("MainDocumentGroup") as DocumentGroup;
      if (mainGroup == null)
      {
        throw new System.Exception("Error retrieving documentGroup from WMSMainDockLayoutManager");
      }
      mainGroup.Add(layoutPanel);
    }
    #endregion
  }    
}
