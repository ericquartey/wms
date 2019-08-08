using Ferretto.Common.BLL.Interfaces.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IMachineLiveData : IModel<int>
    {
        #region Properties

        Enums.MachineStatus Status { get; set; }

        #endregion
    }
}
