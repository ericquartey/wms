using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Machines
{
    public class MachinesViewModel : EntityListViewModel<Machine>
    {
        #region Constructors

        public MachinesViewModel()
        {
            this.FlattenDataSource = true;
        }

        #endregion Constructors
    }
}
