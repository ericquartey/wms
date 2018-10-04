using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public partial class ItemsView : WmsView
    {
        #region Constructors

        public ItemsView()
        {
            this.InitializeComponent();

            this.Loaded += this.ItemsView_Loaded;
            this.MainGridControl.AsyncOperationCompleted += this.MainGridControl_AsyncOperationCompleted;

            SplashScreenService.SetMessage("Initializing main window ...");
        }

        #endregion Constructors

        #region Methods

        private void ItemsView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SplashScreenService.Hide();
        }

        private async void MainGridControl_AsyncOperationCompleted(object sender, System.Windows.RoutedEventArgs e)
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
