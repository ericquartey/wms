using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineProvider
    {
        #region Methods

        void Add(Machine machine);

        void ClearAll();

        Machine Get();

        double GetHeight();

        int GetIdentity();

        MachineStatistics GetPresentStatistics();

        IEnumerable<ServicingInfo> GetServicingInfo();

        IEnumerable<MachineStatistics> GetStatistics();

        void Import(Machine machine, DataLayerContext context);

        void ImportMachineServicingInfo(IEnumerable<ServicingInfo> servicingInfo, DataLayerContext context);

        void ImportMachineStatistics(IEnumerable<MachineStatistics> machineStatistics, DataLayerContext context);

        bool IsOneTonMachine();
        void SetMachineId(int newMachineId);
        void Update(Machine machine, DataLayerContext context);

        void UpdateBayChainStatistics(double distance, BayNumber bayNumber);

        void UpdateBayLoadUnitStatistics(BayNumber bayNumber, int loadUnitId);

        void UpdateHorizontalAxisStatistics(double distance);

        void UpdateMachineServicingInfo(ServicingInfo servicingInfo, DataLayerContext dataContext);

        void UpdateMachineStatistics(MachineStatistics machineStatistics, DataLayerContext dataContext);

        void UpdateMissionTime(TimeSpan duration);

        void UpdateServiceStatistics();

        void UpdateSolo(Machine machine, DataLayerContext dataContext);

        void UpdateVerticalAxisStatistics(double distance);

        void UpdateWeightStatistics(DataLayerContext dataContext);

        #endregion
    }
}
