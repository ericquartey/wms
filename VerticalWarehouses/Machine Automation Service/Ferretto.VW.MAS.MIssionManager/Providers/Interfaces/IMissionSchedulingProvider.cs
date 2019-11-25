using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MissionManager
{
    public interface IMissionSchedulingProvider
    {
        #region Methods

        void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, int? wmsMissionId);

        void QueueCellMission(int loadingUnitId, int targetCellId);

        void QueueLoadingUnitCompactingMission();

        #endregion
    }
}
