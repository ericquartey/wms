using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionOperationPolicy
    {
        #region Properties

        Enums.MissionOperationStatus Status { get; }

        #endregion
    }
}
