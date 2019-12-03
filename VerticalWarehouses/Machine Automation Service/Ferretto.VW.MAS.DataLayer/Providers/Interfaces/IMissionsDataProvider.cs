using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMissionsDataProvider
    {
        #region Methods

        Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber);

        Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, int wmsId, int wmsPriority);

        void Delete(int? id);

        IEnumerable<Mission> GetAllActiveMissionsByBay(BayNumber bayNumber);

        IEnumerable<Mission> GetAllWmsMissions();

        Mission GetExecutingMissionInBay(BayNumber bayNumber);

        Mission SetStatus(int id, MissionStatus status);

        void Update(Mission mission);

        #endregion
    }
}
