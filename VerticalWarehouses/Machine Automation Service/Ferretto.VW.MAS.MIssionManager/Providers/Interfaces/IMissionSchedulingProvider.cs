using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.MissionManager
{
    public interface IMissionSchedulingProvider
    {
        #region Methods

        void AbortMission(Mission localMissionToAbort);

        void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, MissionType missionType);

        void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, int wmsMissionId, int wmsMissionPriority);

        void QueueCellMission(int loadingUnitId, int targetCellId);

        bool QueueFirstTestMission(int loadUnitId, BayNumber sourceBayNumber, int cycle, IServiceProvider serviceProvider);

        bool QueueLoadingUnitCompactingMission(IServiceProvider serviceProvider);

        bool QueueLoadingUnitFastCompactingMission(IServiceProvider serviceProvider);

        void QueueRecallMission(int loadingUnitId, BayNumber sourceBayNumber, MissionType missionType);

        #endregion
    }
}
