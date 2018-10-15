using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Machines
{
    public partial class MachinesView : WmsView
    {
        #region Constructors

        public MachinesView()
        {
            this.InitializeComponent();

            this.MainGridControl.AsyncOperationCompleted += this.MainGridControl_AsyncOperationCompleted;
        }

        #endregion Constructors

        #region Methods

        private async void MainGridControl_AsyncOperationCompleted(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is EntityListViewModel<Machine, int> viewModel)
            {
                await viewModel.UpdateFilterTilesCountsAsync().ConfigureAwait(true);
                this.MainGridControl.AsyncOperationCompleted -= this.MainGridControl_AsyncOperationCompleted;
            }
        }

        #endregion Methods
    }
}
