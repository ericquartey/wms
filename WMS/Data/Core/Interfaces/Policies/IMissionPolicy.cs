using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionPolicy
    {
        #region Properties

        int OperationsCount { get; }

        MissionStatus Status { get; }

        #endregion
    }
}
