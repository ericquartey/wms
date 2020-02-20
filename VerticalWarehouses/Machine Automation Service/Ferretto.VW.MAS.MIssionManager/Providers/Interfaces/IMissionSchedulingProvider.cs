using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.MissionManager
{
    public interface IMissionSchedulingProvider
    {
        #region Methods

        void AbortMission(Mission localMissionToAbort);

        void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber);

        void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, int wmsMissionId, int wmsMissionPriority);

        void QueueCellMission(int loadingUnitId, int targetCellId);

        bool QueueFirstTestMission(int loadUnitId, BayNumber sourceBayNumber, IServiceProvider serviceProvider);

        bool QueueLoadingUnitCompactingMission(IServiceProvider serviceProvider);

        void QueueRecallMission(int loadingUnitId, BayNumber sourceBayNumber);

        #endregion
    }
}
