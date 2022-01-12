using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IServicingProvider
    {
        #region Methods

        void CheckServicingInfo();

        void ConfirmInstruction(int instructionId);

        void ConfirmService(string maintainerName, string note);

        void ConfirmSetup();

        ServicingInfo GetActual();

        IEnumerable<ServicingInfo> GetAll();

        ServicingInfo GetById(int id);

        ServicingInfo GetInstallationInfo();

        ServicingInfo GetLastConfirmed();

        ServicingInfo GetLastValid();

        bool IsAnyInstructionExpired();

        bool IsAnyInstructionExpiring();

        void RefreshDescription(int servicingInfoId);

        void UpdateServiceStatus();

        #endregion
    }
}
