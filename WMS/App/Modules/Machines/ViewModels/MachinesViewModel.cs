using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.Machines
{
    public class MachinesViewModel : EntityListViewModel<Machine>
    {
        #region Constructors

        public MachinesViewModel()
        {
            this.FlattenDataSource = true;
        }

        #endregion
    }
}
