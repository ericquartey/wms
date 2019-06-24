using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces.Policies
{
    public interface IMissionOperationPolicy
    {
        #region Properties

        MissionOperationStatus Status { get; }

        #endregion
    }
}
