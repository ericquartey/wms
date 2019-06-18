using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.Scheduler
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.SchedulerRequest), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Item), false)]
    public class SchedulerRequestsViewModel : EntityPagedListViewModel<SchedulerRequest, int>
    {
        #region Fields

        private bool enableShowDetailsCommand;

        #endregion

        #region Constructors

        public SchedulerRequestsViewModel(IDataSourceService dataSourceService)
          : base(dataSourceService)
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

        private void ShowDetailsCommand_CanExecuteChanged(object sender, System.EventArgs e)
        {
            this.EnableShowDetailsCommand = this.ShowDetailsCommand.CanExecute(null);
        }

        #endregion
    }
}
