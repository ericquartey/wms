using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Modules.BLL;

namespace Ferretto.WMS.Modules.Scheduler
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Mission), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Item), false)]
    public class MissionsViewModel : EntityListViewModel<Mission, int>
    {
        #region Fields

        private bool enableShowDetailsCommand;

        #endregion

        #region Constructors

        public MissionsViewModel(IDataSourceService dataSoruceService)
            : base(dataSoruceService)
        {
            this.ShowDetailsCommand.CanExecuteChanged += this.ShowDetailsCommand_CanExecuteChanged;
        }

        #endregion

        #region Properties

        public bool EnableShowDetailsCommand
        {
            get => this.enableShowDetailsCommand;
            set => this.SetProperty(ref this.enableShowDetailsCommand, value);
        }

        #endregion

        #region Methods

        protected override async Task LoadDataAsync()
        {
            if (this.SelectedFilterDataSource is DataSourceCollection<Mission, int> enumerableSource)
            {
                this.IsBusy = true;
                await enumerableSource.RefreshAsync();
                this.IsBusy = false;
            }
        }

        private void ShowDetailsCommand_CanExecuteChanged(object sender, System.EventArgs e)
        {
            this.EnableShowDetailsCommand = this.ShowDetailsCommand.CanExecute(null);
        }

        #endregion
    }
}
