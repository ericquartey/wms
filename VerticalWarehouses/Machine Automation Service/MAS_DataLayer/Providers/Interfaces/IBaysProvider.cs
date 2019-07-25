using System.Collections.Generic;
using System.Net;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IBaysProvider
    {
        #region Methods

        Bay Activate(int id);

        void Create(Bay bay);

        Bay Deactivate(int id);

        IEnumerable<Bay> GetAll();

        Bay GetById(int id);

        Bay GetByIpAddress(IPAddress remoteIpAddress);

        void Update(int id, string ipAddress, BayType bayType);

        #endregion
    }
}
