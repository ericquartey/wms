using Ferretto.Common.Common_Utils;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS_MissionScheduler
{
    public interface IMissionsScheduler
    {
        #region Methods

        void AddMission(Mission mission);

        void DoHoming(BroadcastDelegate broadcastDelegate);

        #endregion Methods
    }
}
