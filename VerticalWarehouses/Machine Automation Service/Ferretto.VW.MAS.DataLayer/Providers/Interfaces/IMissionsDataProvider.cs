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

        void CheckPendingChanges();

        Mission Complete(int id);

        Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, MissionType missionType, LoadingUnitLocation destination = LoadingUnitLocation.NoLocation);

        Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, int wmsId, int wmsPriority, LoadingUnitLocation destination = LoadingUnitLocation.NoLocation);

        Mission CreateRecallMission(int loadingUnitId, BayNumber bayNumber, MissionType missionType);

        void Delete(int id);

        IEnumerable<Mission> GetAllActiveMissions();

        /// <summary>
        /// Gets the list of new or executing missions, ordered by priority, allocated to the specified bay.
        /// </summary>
        /// <param name="bayNumber">The number of the bay.</param>
        /// <returns>The list of new or executing missions, ordered by priority, allocated to the specified bay.</returns>
        IEnumerable<Mission> GetAllActiveMissionsByBay(BayNumber bayNumber);

        List<int> GetAllActiveUnitGoBay();

        List<int> GetAllActiveUnitGoBay(BayNumber bayNumber);

        List<int> GetAllActiveUnitGoCell();

        List<int> GetAllActiveUnitGoCell(BayNumber bayNumber);

        IEnumerable<Mission> GetAllExecutingMissions();

        IEnumerable<Mission> GetAllMissions();

        IEnumerable<Mission> GetAllWmsMissions();

        Mission GetById(int id);

        Mission GetByWmsId(int id);

        IDbContextTransaction GetContextTransaction();

        bool IsEnabeNoteRules();

        bool IsLocalMachineItems();

        bool IsMissionInWaitState(BayNumber bayNumber, int loadingUnitId);

        bool IsOrderList();

        int PurgeMissions();

        void Reload(Mission mission);

        void ResetMachine(MessageActor sender);

        void Update(Mission mission);

        #endregion
    }
}
