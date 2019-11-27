using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.MissionManager
{
    public interface IMissionSchedulingProvider
    {
        #region Methods

        IEnumerable<Mission> GetAllWmsMissions();

        Task MOCK_ScheduleMissionsAsync(BayNumber bayNumber);

        void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber);

        Task QueueBayMissionAsync(int loadingUnitId, BayNumber targetBayNumber, int wmsMissionId, int wmsMissionPriority);

        void QueueCellMission(int loadingUnitId, int targetCellId);

        void QueueLoadingUnitCompactingMission();

        #endregion
    }
}
