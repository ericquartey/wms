using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.Scheduler
{
    public class MissionsViewModel : EntityPagedListViewModel<Mission, int>
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

        private void ShowDetailsCommand_CanExecuteChanged(object sender, System.EventArgs e)
        {
            this.EnableShowDetailsCommand = this.ShowDetailsCommand.CanExecute(null);
        }

        #endregion
    }
}
