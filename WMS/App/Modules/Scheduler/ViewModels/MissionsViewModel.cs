using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.Scheduler
{
    public class MissionsViewModel : EntityPagedListViewModel<Mission, int>
    {
        #region Fields

        private bool enableShowDetailsCommand;

        #endregion

        #region Properties

        public bool EnableShowDetailsCommand
        {
            get => this.enableShowDetailsCommand;
            set => this.SetProperty(ref this.enableShowDetailsCommand, value);
        }

        #endregion

        #region Methods

        public override void EnableShowDetails()
        {
            this.EnableShowDetailsCommand = this.SelectedItem != null;
        }

        #endregion
    }
}
