using Ferretto.VW.Common_Utils;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS_MissionScheduler
{
    public interface IMissionsScheduler
    {
        #region Methods

        void AddMission(Mission _mission);

        Task DoHoming(BroadcastDelegate _delegate);

        #endregion Methods
    }
}
