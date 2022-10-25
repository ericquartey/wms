using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineProvider
    {
        #region Methods

        void Add(Machine machine);

        void CheckBackupServer();

        Machine Get();

        bool GetAggregateList();

        string GetBackupServer();

        string GetBackupServerPassword();

        string GetBackupServerUsername();

        bool GetBox();

        double GetHeight();

        int GetIdentity();

        int GetItemUniqueIdLength();

        bool GetListPickConfirm();

        bool GetListPutConfirm();

        Machine GetMinMaxHeight();

        MachineStatistics GetPresentStatistics();

        /// <summary>
        /// Get the raw database content.
        /// </summary>
        /// <returns>
        /// The raw database contents (raw bytes)
        /// </returns>
        byte[] GetRawDatabaseContent();

        string GetSecondaryDatabase();

        string GetSerialNumber();

        IEnumerable<ServicingInfo> GetServicingInfo();

        IEnumerable<MachineStatistics> GetStatistics();

        int GetToteBarcodeLength();

        int? GetWaitingListPriorityHighlighted();

        void Import(Machine machine, DataLayerContext context);

        void ImportMachineServicingInfo(IEnumerable<ServicingInfo> servicingInfo, DataLayerContext context);

        void ImportMachineStatistics(IEnumerable<MachineStatistics> machineStatistics, DataLayerContext context);

        bool IsAxisChanged();

        bool IsCanUserEnableWmsEnabled();

        bool IsDbSaveOnServer();

        bool IsDbSaveOnTelemetry();

        bool IsDisableQtyItemEditingPick();

        bool IsEnableAddItem();

        bool IsEnableAddItemDrapery();

        bool IsEnableHandlingItemOperations();

        bool IsFireAlarmActive();

        bool IsHeartBeat();

        bool IsOneTonMachine();

        bool IsRequestConfirmForLastOperationOnLoadingUnit();

        bool IsRotationClassEnabled();

        bool IsTouchHelperEnabled();

        bool IsUpdatingStockByDifference();

        void SetBayOperationParams(Machine machine);

        Task SetMachineId(int newMachineId);

        void Update(Machine machine, DataLayerContext context);

        void UpdateBayChainStatistics(double distance, BayNumber bayNumber);

        void UpdateBayLoadUnitStatistics(BayNumber bayNumber, int loadUnitId);

        void UpdateDbSaveOnServer(bool enable, string server, string username, string password);

        void UpdateDbSaveOnTelemetry(bool enable);

        void UpdateHorizontalAxisStatistics(double distance);

        void UpdateMachineServicingInfo(ServicingInfo servicingInfo, DataLayerContext dataContext);

        void UpdateMachineStatistics(MachineStatistics machineStatistics, DataLayerContext dataContext);

        void UpdateMissionTime(TimeSpan duration);

        void UpdateServiceStatistics();

        void UpdateSolo(Machine machine, DataLayerContext dataContext);

        void UpdateTotalAutomaticTime(TimeSpan duration);

        void UpdateTotalPowerOnTime(TimeSpan duration);

        void UpdateVerticalAxisStatistics(double distance);

        void UpdateWeightStatistics(DataLayerContext dataContext);

        #endregion
    }
}
