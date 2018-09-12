using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Catalog
{
  public partial class ItemDetailsView : WmsView
  {
    #region Constructors

    public ItemDetailsView()
    {
      this.InitializeComponent();

      this.DataContext = new ItemDetailsViewModel();
    }

    #endregion Constructors
  }
}
