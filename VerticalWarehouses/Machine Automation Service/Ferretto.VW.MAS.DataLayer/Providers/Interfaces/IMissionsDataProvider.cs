using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMissionsDataProvider
    {
        #region Methods

        bool CanCreateMission(int loadingUnitId, BayNumber targetBay);

        Mission Complete(int id);

        Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, MissionType missionType);

        Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, int wmsId, int wmsPriority);

        Mission CreateRecallMission(int loadingUnitId, BayNumber bayNumber);

        void Delete(int id);

        IEnumerable<Mission> GetAllActiveMissions();

        /// <summary>
        /// Gets the list of new or executing missions, ordered by priority, allocated to the specified bay.
        /// </summary>
        /// <param name="bayNumber">The number of the bay.</param>
        /// <returns>The list of new or executing missions, ordered by priority, allocated to the specified bay.</returns>
        IEnumerable<Mission> GetAllActiveMissionsByBay(BayNumber bayNumber);

        IEnumerable<Mission> GetAllExecutingMissions();

        IEnumerable<Mission> GetAllMissions();

        IEnumerable<Mission> GetAllWmsMissions();

        Mission GetById(int id);

        IDbContextTransaction GetContextTransaction();

        bool IsMissionInWaitState(BayNumber bayNumber, int loadingUnitId);

        void ResetMachine(MessageActor sender);

        void CheckPendingChanges();
        void Update(Mission mission);

        void UpdateHomingMissions(BayNumber bayNumber, Axis axis);

        #endregion
    }
}
