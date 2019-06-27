using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces.Policies
{
    public interface IMissionPolicy
    {
        #region Properties

        MissionStatus Status { get; }

        #endregion
    }
}
