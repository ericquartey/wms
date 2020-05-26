using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IServicingProvider
    {
        #region Methods

        void CheckServicingInfo();

        void UpdateServiceStatus();

        void ConfirmService();

        void ConfirmSetup();

        ServicingInfo GetActual();

        IEnumerable<ServicingInfo> GetAll();

        ServicingInfo GetById(int id);

        ServicingInfo GetInstallationInfo();

        ServicingInfo GetLastConfirmed();

        #endregion
    }
}
