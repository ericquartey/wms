using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_MissionScheduler
{
    public interface IMissionsScheduler
    {
        #region Methods

        bool AddMission(Mission mission);

        #endregion
    }
}
