using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionOperationPolicy
    {
        #region Properties

        MissionOperationStatus Status { get; }

        #endregion
    }
}
