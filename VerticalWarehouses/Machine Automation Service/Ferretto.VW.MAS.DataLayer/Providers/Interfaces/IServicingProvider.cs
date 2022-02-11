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

        void ConfirmService();

        void ConfirmSetup();

        ServicingInfo GetActual();

        IEnumerable<ServicingInfo> GetAll();

        ServicingInfo GetById(int id);

        ServicingInfo GetInstallationInfo();

        ServicingInfo GetLastConfirmed();

        ServicingInfo GetLastValid();

        MachineStatistics GetSettings(int ID);

        bool IsAnyInstructionExpired();

        bool IsAnyInstructionExpiring();

        void RefreshDescription(int servicingInfoId);

        void SetNote(string maintainerName, string note, int ID);

        void UpdateServiceStatus();

        #endregion
    }
}
