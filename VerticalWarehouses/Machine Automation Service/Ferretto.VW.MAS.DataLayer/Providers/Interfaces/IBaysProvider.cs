using System.Collections.Generic;
using System.Net;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IBaysProvider
    {
        #region Methods

        Bay Activate(int bayNumber);

        Bay AssignMissionOperation(int bayId, int? missionId, int? missionOperationId);

        void Create(Bay bay);

        Bay Deactivate(int bayNumber);

        IEnumerable<Bay> GetAll();

        Bay GetByIpAddress(IPAddress remoteIpAddress);

        Bay GetByNumber(int bayNumber);

        void Update(int bayNumber, string ipAddress, BayType bayType);

        #endregion
    }
}
