using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionPolicy
    {
        #region Properties

        int OperationsCount { get; }

        Enums.MissionStatus Status { get; }

        #endregion
    }
}
