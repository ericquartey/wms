using Ferretto.Common.Controls;
using Ferretto.WMS.App.Core.Models;

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
