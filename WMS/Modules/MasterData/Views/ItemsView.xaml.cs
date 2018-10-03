using DevExpress.Xpf.Core;
using Ferretto.Common.Controls;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public partial class ItemsView : WmsView
    {
        #region Constructors

        public ItemsView()
        {
            this.InitializeComponent();

            this.Loaded += ItemsView_Loaded;
            this.MainGridControl.AsyncOperationCompleted += this.MainGridControl_AsyncOperationCompleted;
        }

        #endregion Constructors

        #region Methods

        private static void ItemsView_Loaded(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (DXSplashScreen.IsActive)
            {
                DXSplashScreen.Close();
            }
        }

        private async void MainGridControl_AsyncOperationCompleted(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is EntityListViewModel<Item> viewModel)
            {
                await viewModel.UpdateFilterTilesCountsAsync().ConfigureAwait(true);
                this.MainGridControl.AsyncOperationCompleted -= this.MainGridControl_AsyncOperationCompleted;
            }
        }

        #endregion Methods
    }
}
