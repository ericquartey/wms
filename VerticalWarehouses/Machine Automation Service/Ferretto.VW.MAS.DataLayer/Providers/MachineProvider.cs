using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MachineProvider : IMachineProvider
    {
        #region Fields

        private const string PASSWORDKEY = "obChaz6W7brGMtT7Dn7TAw==";

        private static readonly Func<DataLayerContext, Machine> MachineGetCompile =
                    EF.CompileQuery((DataLayerContext context) =>
                context.Machines.AsNoTracking()
                    .Include(m => m.Elevator)
                        .ThenInclude(e => e.Axes)
                            .ThenInclude(a => a.EmptyLoadMovement)
                    .Include(m => m.Elevator)
                       .ThenInclude(e => e.Axes)
                           .ThenInclude(a => a.FullLoadMovement)
                    .Include(m => m.Elevator)
                       .ThenInclude(e => e.Axes)
                           .ThenInclude(a => a.WeightMeasurement)
                                .ThenInclude(w => w.WeightDatas)
                    .Include(m => m.Elevator)
                       .ThenInclude(e => e.Axes)
                           .ThenInclude(a => a.AssistedMovements)
                    .Include(m => m.Elevator)
                       .ThenInclude(e => e.Axes)
                           .ThenInclude(a => a.ManualMovements)
                    .Include(m => m.Elevator)
                       .ThenInclude(e => e.Axes)
                           .ThenInclude(a => a.Profiles)
                               .ThenInclude(p => p.Steps)
                    .Include(m => m.Elevator)
                               .ThenInclude(e => e.Axes)
                                   .ThenInclude(a => a.Inverter)
                    .Include(m => m.Elevator)
                        .ThenInclude(e => e.StructuralProperties)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Positions)
                            .ThenInclude(b => b.LoadingUnit)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Carousel)
                            .ThenInclude(b => b.ManualMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Carousel)
                            .ThenInclude(b => b.AssistedMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.External)
                            .ThenInclude(b => b.ManualMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.External)
                            .ThenInclude(b => b.AssistedMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Inverter)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.IoDevice)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.EmptyLoadMovement)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.FullLoadMovement)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                            .ThenInclude(b => b.AssistedMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                            .ThenInclude(b => b.ManualMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                            .ThenInclude(b => b.Inverter)
                    .Include(m => m.Panels)
                        .ThenInclude(p => p.Cells)
                    .Single());

        private readonly IMemoryCache cache;

        private readonly IConfiguration configuration;

        private readonly DataLayerContext dataContext;

        private readonly ILogger<MachineProvider> logger;

        private readonly IMachineVolatileDataProvider machineVolatile;

        private readonly IDbContextRedundancyService<DataLayerContext> redundancyService;

        #endregion

        #region Constructors

        public MachineProvider(
            DataLayerContext dataContext,
            ILogger<MachineProvider> logger,
            IMemoryCache cache,
            IConfiguration configuration,
            IMachineVolatileDataProvider machineVolatile,
            IDbContextRedundancyService<DataLayerContext> redundancyService)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.machineVolatile = machineVolatile ?? throw new ArgumentNullException(nameof(machineVolatile));
            this.redundancyService = redundancyService ?? throw new ArgumentNullException(nameof(redundancyService));
        }

        #endregion

        #region Methods

        public void Add(Machine machine)
        {
            if (machine is null)
            {
                throw new ArgumentNullException(nameof(machine));
            }
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(ElevatorDataProvider.GetAxesCacheKey());
            this.machineVolatile.MachineId = null;
            this.machineVolatile.IsLoadUnitFixed = null;
            lock (this.dataContext)
            {
                this.dataContext.Machines.Add(machine);
                this.dataContext.SaveChanges();
            }
        }

        public void CheckBackupServer()
        {
            lock (this.dataContext)
            {
                var machine = this.dataContext.Machines.First();
                if (machine.BackupServerUsername == null)
                {
                    machine.BackupServerUsername = "wmsadmin";
                }
                if (machine.BackupServerPassword == null)
                {
                    machine.BackupServerPassword = EncryptString(PASSWORDKEY, "fergrp_2012");
                }
                if (machine.WaitingListPriorityHighlighted == null)
                {
                    machine.WaitingListPriorityHighlighted = -1;
                }
                if (machine.IsWaitingListFiltered == null)
                {
                    machine.IsWaitingListFiltered = true;
                }
                this.dataContext.SaveChanges();
            }
        }

        public Machine Get()
        {
            lock (this.dataContext)
            {
                var entity = MachineGetCompile(this.dataContext);
                if (entity.Elevator?.Axes != null)
                {
                    entity.Elevator.Axes = entity.Elevator.Axes.OrderBy(o => o.Orientation);

                    entity.Elevator.Axes.ForEach((axe) =>
                    {
                        axe.Profiles.ForEach((profile) => profile.Steps = profile.Steps.OrderBy(c => c.Number).ToList());
                        if (axe.WeightMeasurement?.WeightDatas != null)
                        {
                            axe.WeightMeasurement.WeightDatas = axe.WeightMeasurement.WeightDatas.OrderBy(o => o.Step);
                        }
                    });
                }

                return entity;
            }
        }

        public bool GetAggregateList()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.AggregateList).First();
            }
        }

        public string GetBackupServer()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.BackupServer).First();
            }
        }

        public string GetBackupServerPassword()
        {
            lock (this.dataContext)
            {
                var password = this.dataContext.Machines.AsNoTracking().Select(m => m.BackupServerPassword).First();
                return DecryptString(PASSWORDKEY, password);
            }
        }

        public string GetBackupServerUsername()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.BackupServerUsername).First();
            }
        }

        public bool GetBox()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.Box).First();
            }
        }

        public double GetHeight()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.Height).First();
            }
        }

        public int GetIdentity()
        {
            if (this.machineVolatile.MachineId is null)
            {
                lock (this.dataContext)
                {
                    this.machineVolatile.MachineId = this.dataContext.Machines.AsNoTracking().Select(m => m.Id).First();
                }
            }
            return this.machineVolatile.MachineId.Value;
        }

        public int GetInverterResponseTimeout()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.InverterResponseTimeout).First();
            }
        }

        public bool GetIsLoadUnitFixed()
        {
            if (this.machineVolatile.IsLoadUnitFixed is null)
            {
                lock (this.dataContext)
                {
                    this.machineVolatile.IsLoadUnitFixed = this.dataContext.Machines.AsNoTracking().Select(m => m.IsLoadUnitFixed).First();
                }
            }
            return this.machineVolatile.IsLoadUnitFixed.Value;
        }

        public int GetItemUniqueIdLength()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.ItemUniqueIdLength).First();
            }
        }

        public bool GetListPickConfirm()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.ListPickConfirm).First();
            }
        }

        public bool GetListPutConfirm()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.ListPutConfirm).First();
            }
        }

        public Machine GetMinMaxHeight()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().First();
            }
        }

        public bool GetMissionOperationSkipable()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.MissionOperationSkipable).First();
            }
        }

        public MachineStatistics GetPresentStatistics()
        {
            lock (this.dataContext)
            {
                return this.dataContext.MachineStatistics.AsNoTracking().ToArray().LastOrDefault();
            }
        }

        public byte[] GetRawDatabaseContent()
        {
            const int NUMBER_OF_RETRIES = 5;
            // Retrieve the path of primary database file
            // example: "Database/MachineAutomationService.Simulation.Primary.db"
            var filePath = GetDBFilePath(this.configuration.GetDataLayerSecondaryConnectionString());
            var exist = File.Exists(filePath);
            if (!exist)
            {
                this.logger.LogError($"Error: {filePath} does not exist");
                return null;
            }
            byte[] rawDatabase = null;
            for (var i = 0; i < NUMBER_OF_RETRIES; i++)
            {
                try
                {
                    lock (this.redundancyService)
                    {
                        // Get the raw bytes contents
                        using (var stream = File.OpenRead(filePath))
                        {
                            rawDatabase = new byte[stream.Length];
                            stream.Read(rawDatabase, 0, rawDatabase.Length);
                        }
                    }
                    break;
                }
                catch (IOException ioExc)
                {
                    if (i >= NUMBER_OF_RETRIES - 1)
                    {
                        throw;
                    }
                    this.logger.LogDebug($"Try: #{i + 1}. Error reason: {ioExc.Message}");
                    Thread.Sleep(10);
                }
                catch (AggregateException ae)
                {
                    ae.Handle(ex =>
                    {
                        if (ex is IOException && i < NUMBER_OF_RETRIES - 1)
                        {
                            this.logger.LogDebug($"Try: #{i + 1}. Error reason aggregate: {ex.Message}");
                            return true;
                        }
                        return false;
                    });
                }
            }
            /*
            // Write the bytes array back to a file
            using (Stream file = File.OpenWrite("Database/trial.db"))
            {
                file.Write(rawDatabase, 0, rawDatabase.Length);
            }
            */
            this.logger.LogInformation($"Retrieve raw secondary (is ok: {this.machineVolatile.IsStandbyDbOk}) database content from file {filePath}");
            return rawDatabase;
        }

        public int GetResponseTimeoutMilliseconds()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.ResponseTimeoutMilliseconds).First();
            }
        }

        public string GetSecondaryDatabase()
        {
            return GetDBFilePath(this.configuration.GetDataLayerSecondaryConnectionString());
        }

        public string GetSerialNumber()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.SerialNumber).First();
            }
        }

        public IEnumerable<ServicingInfo> GetServicingInfo()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ServicingInfo.AsNoTracking();
            }
        }

        public bool GetSimulation()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.Simulation).First();
            }
        }

        public IEnumerable<MachineStatistics> GetStatistics()
        {
            lock (this.dataContext)
            {
                return this.dataContext.MachineStatistics.AsNoTracking();
            }
        }

        public int GetToteBarcodeLength()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.ToteBarcodeLength).First();
            }
        }

        public int GetVerticalPositionToCalibrate()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.VerticalPositionToCalibrate).First();
            }
        }

        public int? GetWaitingListPriorityHighlighted()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.WaitingListPriorityHighlighted).First();
            }
        }

        public void Import(Machine machine, DataLayerContext context)
        {
            _ = machine ?? throw new System.ArgumentNullException(nameof(machine));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(ElevatorDataProvider.GetAxesCacheKey());
            this.machineVolatile.MachineId = null;
            this.machineVolatile.IsExternal = new Dictionary<BayNumber, bool>();
            this.machineVolatile.IsTelescopic = new Dictionary<BayNumber, bool>();
            this.machineVolatile.BayNumbers = new List<BayNumber>();
            this.machineVolatile.IsLoadUnitFixed = null;
            context.ElevatorAxisManualParameters.RemoveRange(context.ElevatorAxisManualParameters);
            context.ShutterManualParameters.RemoveRange(context.ShutterManualParameters);
            context.CarouselManualParameters.RemoveRange(context.CarouselManualParameters);
            context.Carousels.RemoveRange(context.Carousels);
            context.Externals.RemoveRange(context.Externals);
            context.CellPanels.RemoveRange(context.CellPanels);
            context.Shutters.RemoveRange(context.Shutters);
            context.WeightMeasurements.RemoveRange(context.WeightMeasurements);
            context.Inverters.RemoveRange(context.Inverters);
            context.ElevatorStructuralProperties.RemoveRange(context.ElevatorStructuralProperties);
            context.BayPositions.RemoveRange(context.BayPositions);
            context.CellPanels.RemoveRange(context.CellPanels);
            context.Cells.RemoveRange(context.Cells);
            context.IoDevices.RemoveRange(context.IoDevices);
            context.Bays.RemoveRange(context.Bays);
            context.MovementParameters.RemoveRange(context.MovementParameters);
            context.MovementProfiles.RemoveRange(context.MovementProfiles);
            context.ElevatorAxes.RemoveRange(context.ElevatorAxes);
            context.Elevators.RemoveRange(context.Elevators);
            context.Machines.RemoveRange(context.Machines);
            context.Machines.Add(machine);
        }

        public void ImportMachineServicingInfo(IEnumerable<ServicingInfo> servicingInfo, DataLayerContext context)
        {
            _ = servicingInfo ?? throw new System.ArgumentNullException(nameof(servicingInfo));
            context.ServicingInfo.RemoveRange(context.ServicingInfo);
            context.ServicingInfo.AddRange(servicingInfo);
        }

        public void ImportMachineStatistics(IEnumerable<MachineStatistics> machineStatistics, DataLayerContext context)
        {
            _ = machineStatistics ?? throw new System.ArgumentNullException(nameof(machineStatistics));
            context.MachineStatistics.RemoveRange(context.MachineStatistics);
            context.MachineStatistics.AddRange(machineStatistics);
        }

        public bool IsAxisChanged()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsAxisChanged).First();
            }
        }

        public bool IsBackToStartCellEnabled()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsBackToStartCell).First();
            }
        }

        public bool IsCanUserEnableWmsEnabled()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.CanUserEnableWms).First();
            }
        }

        public bool IsDbSaveOnServer()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsDbSaveOnServer).First();
            }
        }

        public bool IsDbSaveOnTelemetry()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsDbSaveOnTelemetry).First();
            }
        }

        public bool IsDisableQtyItemEditingPick()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsDisableQtyItemEditingPick).First();
            }
        }

        public bool IsEnableAddItem()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsEnableAddItem).First();
            }
        }

        public bool IsEnableAddItemDrapery()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsEnableAddItem && m.IsDrapery).First();
            }
        }

        public bool IsEnableHandlingItemOperations()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsEnableHandlingItemOperations).First();
            }
        }

        public bool IsFindMinHeightEnabled()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsFindMinHeight).First();
            }
        }

        public bool IsFireAlarmActive()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.FireAlarm).First();
            }
        }

        public bool IsHeartBeat()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsHeartBeat).First();
            }
        }

        public bool IsHeightAlarmActive()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.HeightAlarm).First();
            }
        }

        public bool IsOneTonMachine()
        {
            lock (this.dataContext)
            {
                var elevatorInvertersCount = this.dataContext.ElevatorAxes
                    .Where(a => a.Inverter != null)
                    .Select(a => a.Inverter.Id)
                    .Distinct()
                    .Count();
                return elevatorInvertersCount > 1;
            }
        }

        public bool IsOstecActive()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsOstec).First();
            }
        }

        public bool IsRequestConfirmForLastOperationOnLoadingUnit()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsRequestConfirmForLastOperationOnLoadingUnit).First();
            }
        }

        public bool IsRotationClassEnabled()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsRotationClass).First();
            }
        }

        public bool IsSensitiveCarpetsBypass()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.SensitiveCarpetsAlarm).First();
            }
        }

        public bool IsSensitiveEdgeBypass()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.SensitiveEdgeAlarm).First();
            }
        }

        public bool IsSilenceSirenAlarm()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.SilenceSirenAlarm).First();
            }
        }

        //public bool IsHeightAlarm()
        //{
        //    lock (this.dataContext)
        //    {
        //        return this.dataContext.Machines.AsNoTracking().Select(m => m.HeightAlarm).First();
        //    }
        //}
        public bool IsSpeaActive()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsSpea).First();
            }
        }

        public bool IsTouchHelperEnabled()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.TouchHelper).First();
            }
        }

        public bool IsUpdatingStockByDifference()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsUpdatingStockByDifference).First();
            }
        }

        public void SetBayOperationParams(Machine machine)
        {
            lock (this.dataContext)
            {
                var machineDB = this.dataContext.Machines.First();
                machineDB.IsEnableHandlingItemOperations = machine.IsEnableHandlingItemOperations;
                machineDB.IsUpdatingStockByDifference = machine.IsUpdatingStockByDifference;
                machineDB.IsRequestConfirmForLastOperationOnLoadingUnit = machine.IsRequestConfirmForLastOperationOnLoadingUnit;
                machineDB.IsEnableAddItem = machine.IsEnableAddItem;
                machineDB.IsDisableQtyItemEditingPick = machine.IsDisableQtyItemEditingPick;
                machineDB.IsDoubleConfirmBarcodeInventory = machine.IsDoubleConfirmBarcodeInventory;
                machineDB.IsDoubleConfirmBarcodePick = machine.IsDoubleConfirmBarcodePick;
                machineDB.IsDoubleConfirmBarcodePut = machine.IsDoubleConfirmBarcodePut;
                machineDB.Box = machine.Box;
                machineDB.EnabeNoteRules = machine.EnabeNoteRules;
                machineDB.IsLocalMachineItems = machine.IsLocalMachineItems;
                machineDB.IsOrderList = machine.IsOrderList;
                machineDB.ItemUniqueIdLength = machine.ItemUniqueIdLength;
                machineDB.ToteBarcodeLength = machine.ToteBarcodeLength;
                machineDB.IsDrapery = machine.IsDrapery;
                machineDB.IsCarrefour = machine.IsCarrefour;
                machineDB.IsAsendia = machine.IsAsendia;
                machineDB.IsBypassReason = machine.IsBypassReason;
                machineDB.LotFilter = machine.LotFilter;
                machineDB.ShowWaitListInOperation = machine.ShowWaitListInOperation;
                machineDB.IsItalMetal = machine.IsItalMetal;
                machineDB.IsOstec = machine.IsOstec;
                machineDB.IsSpea = machine.IsSpea;
                machineDB.ShowQuantityOnInventory = machine.ShowQuantityOnInventory;
                machineDB.IsQuantityLimited = machine.IsQuantityLimited;
                machineDB.IsAddItemByList = machine.IsAddItemByList;
                machineDB.WaitingListPriorityHighlighted = machine.WaitingListPriorityHighlighted;
                machineDB.ListPickConfirm = machine.ListPickConfirm;
                machineDB.ListPutConfirm = machine.ListPutConfirm;
                machineDB.AggregateList = machine.AggregateList;
                machineDB.IsWaitingListFiltered = machine.IsWaitingListFiltered;
                machineDB.OperationRightToLeft = machine.OperationRightToLeft;
                machineDB.FixedPick = machine.FixedPick;
                machineDB.MissionOperationSkipable = machine.MissionOperationSkipable;

                this.dataContext.SaveChanges();
            }
        }

        public async Task SetHeightAlarm(bool value)
        {
            lock (this.dataContext)
            {
                this.dataContext.Machines.First().HeightAlarm = value;
                this.dataContext.SaveChanges();
            }
        }

        public async Task SetInverterResponseTimeout(int value)
        {
            lock (this.dataContext)
            {
                this.dataContext.Machines.First().InverterResponseTimeout = value;
                this.dataContext.SaveChanges();
            }
        }

        public async Task SetMachineId(int newMachineId)
        {
            DataLayerContext dataContext;
            lock (this.dataContext)
            {
                dataContext = this.dataContext;
                this.machineVolatile.MachineId = null;
                this.machineVolatile.IsLoadUnitFixed = null;
            }
            int count = await dataContext.Database.ExecuteSqlCommandAsync("update cellpanels set MachineId = null;");
            int count1 = await dataContext.Database.ExecuteSqlCommandAsync("update bays set MachineId = null;");
            int count2 = await dataContext.Database.ExecuteSqlCommandAsync($"update machines set Id = {newMachineId};");
            int count3 = await dataContext.Database.ExecuteSqlCommandAsync($"update cellpanels set MachineId = {newMachineId};");
            int count4 = await dataContext.Database.ExecuteSqlCommandAsync($"update bays set MachineId = {newMachineId};");
        }

        public async Task SetResponseTimeoutMilliseconds(int value)
        {
            lock (this.dataContext)
            {
                this.dataContext.Machines.First().ResponseTimeoutMilliseconds = value;
                this.dataContext.SaveChanges();
            }
        }

        public async Task SetSensitiveCarpetsBypass(bool value)
        {
            lock (this.dataContext)
            {
                this.dataContext.Machines.First().SensitiveCarpetsAlarm = value;
                this.dataContext.SaveChanges();
            }
        }

        public async Task SetSensitiveEdgeBypass(bool value)
        {
            lock (this.dataContext)
            {
                this.dataContext.Machines.First().SensitiveEdgeAlarm = value;
                this.dataContext.SaveChanges();
            }
        }

        public async Task SetSilenceSirenAlarm(bool silenceSirenAlarm)
        {
            lock (this.dataContext)
            {
                this.dataContext.Machines.First().SilenceSirenAlarm = silenceSirenAlarm;
                this.dataContext.SaveChanges();
            }
        }

        public void Update(Machine machine, DataLayerContext dataContext)
        {
            _ = machine ?? throw new System.ArgumentNullException(nameof(machine));
            if (dataContext is null)
            {
                dataContext = this.dataContext;
            }
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(ElevatorDataProvider.GetAxesCacheKey());
            this.machineVolatile.MachineId = null;
            this.machineVolatile.BayNumbers = new List<BayNumber>();
            this.machineVolatile.IsExternal = new Dictionary<BayNumber, bool>();
            this.machineVolatile.IsTelescopic = new Dictionary<BayNumber, bool>();
            this.machineVolatile.IsLoadUnitFixed = null;
            machine.Elevator?.Axes.ForEach((a) =>
            {
                dataContext.AddOrUpdate(a.EmptyLoadMovement, (e) => e.Id);
                dataContext.AddOrUpdate(a.FullLoadMovement, (e) => e.Id);
                dataContext.AddOrUpdate(a.WeightMeasurement, (e) => e.Id);
                dataContext.AddOrUpdate(a.AssistedMovements, (e) => e.Id);
                dataContext.AddOrUpdate(a.ManualMovements, (e) => e.Id);
                dataContext.AddOrUpdate(a.Inverter, (e) => e.Id);
                a.Profiles.ForEach((p) =>
                {
                    p.Steps.ForEach((s) => dataContext.AddOrUpdate(s, (e) => e.Id));
                    dataContext.AddOrUpdate(p, (e) => e.Id);
                });
                dataContext.AddOrUpdate(a, (e) => e.Id);
            });
            dataContext.AddOrUpdate(machine.Elevator?.StructuralProperties, (e) => e.Id);
            dataContext.AddOrUpdate(machine.Elevator, (e) => e.Id);
            machine.Bays.ForEach((b) =>
            {
                b.Positions.ForEach((p) =>
                {
                    dataContext.AddOrUpdate(p.LoadingUnit, (e) => e.Id);
                    dataContext.AddOrUpdate(p, (e) => e.Id);
                });
                dataContext.AddOrUpdate(b.Carousel, (e) => e.Id);
                dataContext.AddOrUpdate(b.Carousel?.AssistedMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.Carousel?.ManualMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.External, (e) => e.Id);
                dataContext.AddOrUpdate(b.External?.AssistedMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.External?.ManualMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.EmptyLoadMovement, (e) => e.Id);
                dataContext.AddOrUpdate(b.FullLoadMovement, (e) => e.Id);
                dataContext.AddOrUpdate(b.Inverter, (e) => e.Id);
                dataContext.AddOrUpdate(b.IoDevice, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter?.Inverter, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter?.AssistedMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter?.ManualMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b, (e) => e.Id);
            });
            machine.Panels.ForEach((p) =>
            {
                p.Cells.ForEach((c) => dataContext.AddOrUpdate(c, (e) => e.Id));
                dataContext.AddOrUpdate(p, (e) => e.Id);
            });
            dataContext.AddOrUpdate(machine, (e) => e.Id);
            dataContext.SaveChanges();
        }

        public void UpdateBayChainStatistics(double distance, BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var machineStat = this.dataContext.MachineStatistics.ToArray().LastOrDefault();
                if (machineStat != null)
                {
                    switch (bayNumber)
                    {
                        case BayNumber.BayOne:
                            machineStat.TotalBayChainKilometers1 += distance / 1000000;
                            break;

                        case BayNumber.BayTwo:
                            machineStat.TotalBayChainKilometers2 += distance / 1000000;
                            break;

                        case BayNumber.BayThree:
                            machineStat.TotalBayChainKilometers3 += distance / 1000000;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(bayNumber));
                    }
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateBayLoadUnitStatistics(BayNumber bayNumber, int loadUnitId)
        {
            lock (this.dataContext)
            {
                var machineStat = this.dataContext.MachineStatistics.ToArray().LastOrDefault();
                if (machineStat != null)
                {
                    switch (bayNumber)
                    {
                        case BayNumber.BayOne:
                            machineStat.TotalLoadUnitsInBay1++;
                            break;

                        case BayNumber.BayTwo:
                            machineStat.TotalLoadUnitsInBay2++;
                            break;

                        case BayNumber.BayThree:
                            machineStat.TotalLoadUnitsInBay3++;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(bayNumber));
                    }
                    var loadUnit = this.dataContext.LoadingUnits.FirstOrDefault(l => l.Id == loadUnitId);
                    if (loadUnit != null)
                    {
                        loadUnit.MissionsCount++;
                    }
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateDbSaveOnServer(bool enable, string server, string username, string password)
        {
            lock (this.dataContext)
            {
                var machine = this.dataContext.Machines.FirstOrDefault();
                var passwordEncrypt = EncryptString(PASSWORDKEY, password);
                if (machine != null)
                {
                    machine.IsDbSaveOnServer = enable;
                    machine.BackupServer = server;
                    machine.BackupServerUsername = username;
                    machine.BackupServerPassword = passwordEncrypt;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateDbSaveOnTelemetry(bool enable)
        {
            lock (this.dataContext)
            {
                var machine = this.dataContext.Machines.FirstOrDefault();
                if (machine != null)
                {
                    machine.IsDbSaveOnTelemetry = enable;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateHorizontalAxisStatistics(double distance)
        {
            lock (this.dataContext)
            {
                var machineStat = this.dataContext.MachineStatistics.ToArray().LastOrDefault();
                if (machineStat != null)
                {
                    machineStat.TotalHorizontalAxisCycles++;
                    machineStat.TotalHorizontalAxisKilometers += distance / 1000000;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateMachineServicingInfo(ServicingInfo servicingInfo, DataLayerContext dataContext)
        {
            _ = servicingInfo ?? throw new System.ArgumentNullException(nameof(servicingInfo));
            if (dataContext is null)
            {
                dataContext = this.dataContext;
            }
            dataContext.AddOrUpdate(servicingInfo, (e) => e.Id);
            dataContext.SaveChanges();
        }

        public void UpdateMachineStatistics(MachineStatistics machineStatistics, DataLayerContext dataContext)
        {
            _ = machineStatistics ?? throw new System.ArgumentNullException(nameof(machineStatistics));
            if (dataContext is null)
            {
                dataContext = this.dataContext;
            }
            dataContext.AddOrUpdate(machineStatistics, (e) => e.Id);
            dataContext.SaveChanges();
        }

        public void UpdateMissionTime(TimeSpan duration)
        {
            lock (this.dataContext)
            {
                var machineStat = this.dataContext.MachineStatistics.ToArray().LastOrDefault();
                if (machineStat != null)
                {
                    machineStat.TotalMissionTime = machineStat.TotalMissionTime + duration;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateServiceStatistics()
        {
            lock (this.dataContext)
            {
                var servicingInfo = this.dataContext.ServicingInfo.ToArray().LastOrDefault();
                if (servicingInfo != null)
                {
                    servicingInfo.TotalMissions = (servicingInfo.TotalMissions.HasValue ? servicingInfo.TotalMissions + 1 : 1);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateSolo(Machine machine, DataLayerContext dataContext)
        {
            _ = machine ?? throw new ArgumentNullException(nameof(machine));
            if (dataContext is null)
            {
                dataContext = this.dataContext;
            }
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(ElevatorDataProvider.GetAxesCacheKey());
            this.machineVolatile.MachineId = null;
            this.machineVolatile.BayNumbers = new List<BayNumber>();
            this.machineVolatile.IsExternal = new Dictionary<BayNumber, bool>();
            this.machineVolatile.IsTelescopic = new Dictionary<BayNumber, bool>();
            this.machineVolatile.IsLoadUnitFixed = null;

            machine.Elevator?.Axes.ForEach((a) =>
            {
                dataContext.AddOrUpdate(a.EmptyLoadMovement, (e) => e.Id);
                dataContext.AddOrUpdate(a.FullLoadMovement, (e) => e.Id);
                dataContext.AddOrUpdate(a.WeightMeasurement, (e) => e.Id);
                dataContext.AddOrUpdate(a.AssistedMovements, (e) => e.Id);
                dataContext.AddOrUpdate(a.ManualMovements, (e) => e.Id);
                dataContext.AddOrUpdate(a.Inverter, (e) => e.Id);
                a.Profiles.ForEach((p) =>
                {
                    p.Steps.ForEach((s) => dataContext.AddOrUpdate(s, (e) => e.Id));
                    dataContext.AddOrUpdate(p, (e) => e.Id);
                });
                dataContext.AddOrUpdate(a, (e) => e.Id);
            });
            dataContext.AddOrUpdate(machine.Elevator?.StructuralProperties, (e) => e.Id);
            dataContext.AddOrUpdate(machine.Elevator, (e) => e.Id);
            machine.Bays.ForEach((b) =>
            {
                b.Positions.ForEach((p) =>
                {
                    dataContext.AddOrUpdate(p.LoadingUnit, (e) => e.Id);
                    dataContext.AddOrUpdate(p, (e) => e.Id);
                });
                dataContext.AddOrUpdate(b.Carousel, (e) => e.Id);
                dataContext.AddOrUpdate(b.Carousel?.AssistedMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.Carousel?.ManualMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.External, (e) => e.Id);
                dataContext.AddOrUpdate(b.External?.AssistedMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.External?.ManualMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.EmptyLoadMovement, (e) => e.Id);
                dataContext.AddOrUpdate(b.FullLoadMovement, (e) => e.Id);
                dataContext.AddOrUpdate(b.Inverter, (e) => e.Id);
                dataContext.AddOrUpdate(b.IoDevice, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter?.Inverter, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter?.AssistedMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter?.ManualMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b, (e) => e.Id);
            });
            //machine.Panels.ForEach((p) =>
            //{
            //    p.Cells.ForEach((c) => dataContext.AddOrUpdate(c, (e) => e.Id));
            //    dataContext.AddOrUpdate(p, (e) => e.Id);
            //});
            dataContext.AddOrUpdate(machine, (e) => e.Id);
            dataContext.SaveChanges();

            if (!machine.IsLoadUnitFixed)
            {
                this.FreeReservedCells(machine, dataContext);
                this.FreeFixedLoadUnits(machine, dataContext);
            }
        }

        public void UpdateTotalAutomaticTime(TimeSpan duration)
        {
            lock (this.dataContext)
            {
                var machineStat = this.dataContext.MachineStatistics.ToArray().LastOrDefault();
                if (machineStat != null)
                {
                    machineStat.TotalAutomaticTime += duration;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateTotalPowerOnTime(TimeSpan duration)
        {
            lock (this.dataContext)
            {
                var machineStat = this.dataContext.MachineStatistics.ToArray().LastOrDefault();
                if (machineStat != null)
                {
                    machineStat.TotalPowerOnTime += duration;
                    this.dataContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Update vertical axis statistics
        /// </summary>
        /// <param name="distance">
        /// space in millimeters
        /// </param>
        public void UpdateVerticalAxisStatistics(double distance)
        {
            lock (this.dataContext)
            {
                var machineStat = this.dataContext.MachineStatistics.ToArray().LastOrDefault();
                if (machineStat != null)
                {
                    machineStat.TotalVerticalAxisCycles++;
                    machineStat.TotalVerticalAxisKilometers += distance / 1000000;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateWeightStatistics(DataLayerContext dataContext)
        {
            var machineStat = dataContext.MachineStatistics.ToArray().LastOrDefault();
            machineStat.TotalWeightFront = 0;
            machineStat.TotalWeightBack = 0;
            var loadingUnits = dataContext.LoadingUnits
                .Include(i => i.Cell)
                .ThenInclude(t => t.Panel)
                .ToList();
            loadingUnits.ForEach((l) =>
            {
                if (l.Cell != null)
                {
                    if (l.Cell.Side == WarehouseSide.Front)
                    {
                        machineStat.TotalWeightFront += l.GrossWeight;
                    }
                    else
                    {
                        machineStat.TotalWeightBack += l.GrossWeight;
                    }
                    if (l.Status != DataModels.Enumerations.LoadingUnitStatus.Blocked)
                    {
                        l.Status = DataModels.Enumerations.LoadingUnitStatus.InLocation;
                    }
                }
            }
            );
            dataContext.SaveChanges();
        }

        private static string DecryptString(string key, string cipherPassword)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherPassword);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        private static string EncryptString(string key, string password)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(password);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        /// <summary>
        /// Retrieve the path of the primary database.
        /// </summary>
        /// <param name="primaryConnectionString">
        /// </param>
        /// <returns>
        /// The path
        /// </returns>
        private static string GetDBFilePath(string primaryConnectionString)
        {
            try
            {
                var index = primaryConnectionString.IndexOf("'", StringComparison.CurrentCulture);

                var tmp = primaryConnectionString.Remove(0, index + 1);
                index = tmp.IndexOf("'", StringComparison.CurrentCulture);

                var retValue = tmp.Remove(index, 1);
                return retValue;
            }
            catch (Exception)
            {
            }

            return null;
        }

        private void FreeFixedLoadUnits(Machine machine, DataLayerContext dataContext)
        {
            var loadUnits = dataContext.LoadingUnits.Where(lu => lu.IsCellFixed).ToList();
            if (loadUnits.Count > 0)
            {
                loadUnits.ForEach(lu =>
                {
                    lu.FixedCell = null;
                    lu.FixedHeight = null;
                    lu.IsCellFixed = false;
                    lu.IsHeightFixed = false;
                });
                dataContext.SaveChanges();
            }
        }

        private void FreeReservedCells(Machine machine, DataLayerContext dataContext)
        {
            var cells = dataContext.Cells.Where(c => c.BlockLevel == BlockLevel.Reserved).ToList();
            if (cells.Count > 0)
            {
                cells.ForEach(cell => cell.BlockLevel = BlockLevel.None);
                dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
