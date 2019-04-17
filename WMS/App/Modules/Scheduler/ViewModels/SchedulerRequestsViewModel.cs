using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.Scheduler
{
    public class SchedulerRequestsViewModel : EntityPagedListViewModel<SchedulerRequest, int>
    {
        #region Fields

        private bool enableShowDetailsCommand;

        #endregion

        #region Constructors

        public SchedulerRequestsViewModel()
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
