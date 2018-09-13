using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Catalog
{
    public partial class ItemsView : WmsView
    {
        #region Constructors

        public ItemsView()
        {
            this.InitializeComponent();

            this.DataContext = new ItemsViewModel();
        }

        #endregion Constructors
    }
}
