using DevExpress.Xpf.Core;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.MasterData
{
    public partial class ItemsView : WmsView
    {
        #region Constructors

        public ItemsView()
        {
            this.InitializeComponent();

            this.Loaded += this.ItemsView_Loaded;
        }

        #endregion Constructors

        #region Methods

        private void ItemsView_Loaded(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (DXSplashScreen.IsActive)
            {
                DXSplashScreen.Close();
            }
        }

        #endregion Methods
    }
}
