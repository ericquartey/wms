using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_MissionScheduler
{
    public interface IMissionsScheduler
    {
        #region Methods

        void AddMission(Mission mission);

        void DoHoming();

        void Test01();

        void Test02();

        #endregion
    }
}
