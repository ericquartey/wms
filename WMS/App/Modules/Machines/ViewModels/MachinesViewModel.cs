using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Machines
{
    public class MachinesViewModel : EntityPagedListViewModel<Machine, int>
    {
        #region Constructors

        public MachinesViewModel()
        {
            this.FlattenDataSource = true;
        }

        #endregion
    }
}
