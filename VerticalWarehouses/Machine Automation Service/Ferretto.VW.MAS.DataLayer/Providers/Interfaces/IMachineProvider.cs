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

        bool GetIsLoadUnitFixed();

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

        int GetResponseTimeoutMilliseconds();

        string GetSecondaryDatabase();

        string GetSerialNumber();

        IEnumerable<ServicingInfo> GetServicingInfo();

        bool GetSimulation();

        IEnumerable<MachineStatistics> GetStatistics();

        int GetToteBarcodeLength();

        int GetVerticalPositionToCalibrate();

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

        bool IsHeightAlarmActive();

        bool IsOneTonMachine();

        bool IsOstecActive();

        bool IsRequestConfirmForLastOperationOnLoadingUnit();

        bool IsRotationClassEnabled();

        bool IsSensitiveCarpetsBypass();

        bool IsSensitiveEdgeBypass();

        bool IsSilenceSirenAlarm();

        bool IsSpeaActive();

        bool IsTouchHelperEnabled();

        bool IsUpdatingStockByDifference();

        void SetBayOperationParams(Machine machine);

        Task SetHeightAlarm(bool value);

        Task SetMachineId(int newMachineId);

        Task SetResponseTimeoutMilliseconds(int value);

        Task SetSensitiveCarpetsBypass(bool value);

        Task SetSensitiveEdgeBypass(bool value);

        Task SetSilenceSirenAlarm(bool silenceSirenAlarm);

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
