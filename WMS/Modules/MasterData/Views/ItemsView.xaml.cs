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

            this.MainGridControl.AsyncOperationCompleted += this.MainGridControl_AsyncOperationCompleted;

            SplashScreenService.SetMessage(Common.Resources.DesktopApp.InitializingMainWindow);
        }

        #endregion Constructors

        #region Methods

        private async void MainGridControl_AsyncOperationCompleted(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is EntityListViewModel<Item, int> viewModel)
            {
                await viewModel.UpdateFilterTilesCountsAsync().ConfigureAwait(true);
                this.MainGridControl.AsyncOperationCompleted -= this.MainGridControl_AsyncOperationCompleted;
            }
        }

        #endregion Methods
    }
}
