using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMissionsDataProvider
    {
        #region Methods

        bool CanCreateMission(int loadingUnitId, BayNumber targetBay);

        Mission Complete(int id);

        Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber);

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

        IEnumerable<Mission> GetAllExecutingMissions(bool noCache = false);

        IEnumerable<Mission> GetAllWmsMissions();

        Mission GetByGuid(Guid fsmId);

        Mission GetById(int id);

        bool IsMissionInWaitState(BayNumber bayNumber, int loadingUnitId);

        void ResetMachine(MessageActor sender);

        void Update(Mission mission);

        void UpdateHomingMissions(BayNumber bayNumber, Axis axis);

        #endregion
    }
}
