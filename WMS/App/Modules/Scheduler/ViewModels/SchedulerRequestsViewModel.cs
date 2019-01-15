using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.Scheduler
{
    public class SchedulerRequestsViewModel : EntityListViewModel<SchedulerRequest>
    {
        #region Fields

        private ICommand refreshCommand;

        #endregion Fields

        #region Properties

        public ICommand RefreshCommand => this.refreshCommand ??
                    (this.refreshCommand = new DelegateCommand(
                this.ExecuteRefreshCommand));

        #endregion Properties

        #region Methods

        private void ExecuteRefreshCommand()
        {
            this.LoadRelatedData();
        }

        #endregion Methods
    }
}
