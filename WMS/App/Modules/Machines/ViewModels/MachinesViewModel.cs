using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.Machines
{
    public class MachinesViewModel : EntityListViewModel<Machine>
    {
        #region Fields

        private ICommand refreshCommand;

        #endregion Fields

        #region Constructors

        public MachinesViewModel()
        {
            this.FlattenDataSource = true;
        }

        #endregion Constructors

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
