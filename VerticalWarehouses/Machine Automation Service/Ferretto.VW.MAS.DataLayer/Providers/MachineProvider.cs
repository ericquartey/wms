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
                                        .ThenInclude(a => a.Parameters)
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
                        .ThenInclude(b => b.Inverter)
                            .ThenInclude(a => a.Parameters)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                            .ThenInclude(b => b.AssistedMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                            .ThenInclude(b => b.ManualMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                            .ThenInclude(b => b.Inverter)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                            .ThenInclude(b => b.Inverter)
                                .ThenInclude(b => b.Parameters)
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
            this.redundancyService = redundancyService ?? throw new System.ArgumentNullException(nameof(redundancyService));
        }

        #endregion

        #region Methods

        public void Add(Machine machine)
        {
            if (machine is null)
            {
                throw new System.ArgumentNullException(nameof(machine));
            }
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(ElevatorDataProvider.GetAxesCacheKey());
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
                var machine = this.dataContext.Machines.FirstOrDefault();
                if (machine.BackupServerUsername == null)
                {
                    machine.BackupServerUsername = "wmsadmin";
                }
                if (machine.BackupServerPassword == null)
                {
                    machine.BackupServerPassword = EncryptString(PASSWORDKEY, "fergrp_2012");
                }
                this.dataContext.SaveChanges();
            }
        }

        public void ClearAll()
        {
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(ElevatorDataProvider.GetAxesCacheKey());

            lock (this.dataContext)
            {
                this.dataContext.Shutters.RemoveRange(this.dataContext.Shutters);
                this.dataContext.WeightMeasurements.RemoveRange(this.dataContext.WeightMeasurements);
                this.dataContext.Inverters.RemoveRange(this.dataContext.Inverters);
                this.dataContext.ElevatorStructuralProperties.RemoveRange(this.dataContext.ElevatorStructuralProperties);
                this.dataContext.LoadingUnits.RemoveRange(this.dataContext.LoadingUnits);
                this.dataContext.BayPositions.RemoveRange(this.dataContext.BayPositions);
                this.dataContext.CellPanels.RemoveRange(this.dataContext.CellPanels);
                this.dataContext.Cells.RemoveRange(this.dataContext.Cells);
                this.dataContext.IoDevices.RemoveRange(this.dataContext.IoDevices);
                this.dataContext.Bays.RemoveRange(this.dataContext.Bays);
                this.dataContext.MovementParameters.RemoveRange(this.dataContext.MovementParameters);
                this.dataContext.MovementProfiles.RemoveRange(this.dataContext.MovementProfiles);
                this.dataContext.ElevatorAxes.RemoveRange(this.dataContext.ElevatorAxes);
                this.dataContext.Elevators.RemoveRange(this.dataContext.Elevators);
                this.dataContext.Machines.RemoveRange(this.dataContext.Machines);
                this.dataContext.SetupProcedures.RemoveRange(this.dataContext.SetupProcedures);
                this.dataContext.SetupProceduresSets.RemoveRange(this.dataContext.SetupProceduresSets);
                this.dataContext.SaveChanges();
            }
        }

        public Machine Get()
        {
            lock (this.dataContext)
            {
                var entity = MachineGetCompile(this.dataContext);

                entity.Elevator?.Axes?.ForEach((axe) =>
                {
                    axe.Profiles.ForEach((profile) => profile.Steps = profile.Steps.OrderBy(c => c.Number).ToList());
                });

                return entity;
            }
        }

        public string GetBackupServer()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault().BackupServer;
            }
        }

        public string GetBackupServerPassword()
        {
            lock (this.dataContext)
            {
                var password = this.dataContext.Machines.FirstOrDefault().BackupServerPassword;
                return DecryptString(PASSWORDKEY, password);
            }
        }

        public string GetBackupServerUsername()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault().BackupServerUsername;
            }
        }

        public bool GetBox()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault().Box;
            }
        }

        public double GetHeight()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.Select(m => m.Height).Single();
            }
        }

        public int GetIdentity()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.Select(m => m.Id).Single();
            }
        }

        public int GetItemUniqueIdLength()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault().ItemUniqueIdLength;
            }
        }

        public Machine GetMinMaxHeight()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault();
            }
        }

        public MachineStatistics GetPresentStatistics()
        {
            lock (this.dataContext)
            {
                return this.dataContext.MachineStatistics.AsNoTracking().LastOrDefault();
            }
        }

        public byte[] GetRawDatabaseContent()
        {
            const int NUMBER_OF_RETRIES = 5;
            // Retrieve the path of primary database file
            //      example: "Database/MachineAutomationService.Simulation.Primary.db"
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

        public string GetSecondaryDatabase()
        {
            return GetDBFilePath(this.configuration.GetDataLayerSecondaryConnectionString());
        }

        public string GetSerialNumber()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault().SerialNumber;
            }
        }

        public IEnumerable<ServicingInfo> GetServicingInfo()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ServicingInfo.AsNoTracking();
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
                return this.dataContext.Machines.FirstOrDefault().ToteBarcodeLength;
            }
        }

        public void Import(Machine machine, DataLayerContext context)
        {
            _ = machine ?? throw new System.ArgumentNullException(nameof(machine));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(ElevatorDataProvider.GetAxesCacheKey());
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
                return this.dataContext.Machines.FirstOrDefault()?.IsAxisChanged ?? false;
            }
        }

        public bool IsDbSaveOnServer()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault()?.IsDbSaveOnServer ?? false;
            }
        }

        public bool IsDbSaveOnTelemetry()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault()?.IsDbSaveOnTelemetry ?? false;
            }
        }

        public bool IsDisableQtyItemEditingPick()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault()?.IsDisableQtyItemEditingPick ?? false;
            }
        }

        public bool IsEnableAddItem()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault()?.IsEnableAddItem ?? false;
            }
        }

        public bool IsEnableAddItemDrapery()
        {
            lock (this.dataContext)
            {
                var machine = this.dataContext.Machines.FirstOrDefault();
                return machine != null && machine.IsEnableAddItem && machine.IsDrapery;
            }
        }

        public bool IsEnableHandlingItemOperations()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault()?.IsEnableHandlingItemOperations ?? false;
            }
        }

        public bool IsFireAlarmActive()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault()?.FireAlarm ?? false;
            }
        }

        public bool IsHeartBeat()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault()?.IsHeartBeat ?? false;
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

        public bool IsRequestConfirmForLastOperationOnLoadingUnit()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault()?.IsRequestConfirmForLastOperationOnLoadingUnit ?? false;
            }
        }

        public bool IsTouchHelperEnabled()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault().TouchHelper;
            }
        }

        public bool IsUpdatingStockByDifference()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.FirstOrDefault()?.IsUpdatingStockByDifference ?? false;
            }
        }

        public void SetBayOperationParams(Machine machine)
        {
            lock (this.dataContext)
            {
                var machineDB = this.dataContext.Machines.LastOrDefault();
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
                machineDB.IsQuantityLimited = machine.IsQuantityLimited;
                this.dataContext.SaveChanges();
            }
        }

        public async Task SetMachineId(int newMachineId)
        {
            DataLayerContext dataContext;
            lock (this.dataContext)
            {
                dataContext = this.dataContext;
            }
            int count = await dataContext.Database.ExecuteSqlCommandAsync("update cellpanels set MachineId = null;");
            int count1 = await dataContext.Database.ExecuteSqlCommandAsync("update bays set MachineId = null;");
            int count2 = await dataContext.Database.ExecuteSqlCommandAsync($"update machines set Id = {newMachineId};");
            int count3 = await dataContext.Database.ExecuteSqlCommandAsync($"update cellpanels set MachineId = {newMachineId};");
            int count4 = await dataContext.Database.ExecuteSqlCommandAsync($"update bays set MachineId = {newMachineId};");
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
                var machineStat = this.dataContext.MachineStatistics.LastOrDefault();
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
                var machineStat = this.dataContext.MachineStatistics.LastOrDefault();
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
                var machineStat = this.dataContext.MachineStatistics.LastOrDefault();
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
                var machineStat = this.dataContext.MachineStatistics.LastOrDefault();
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
                var servicingInfo = this.dataContext.ServicingInfo.LastOrDefault();
                if (servicingInfo != null)
                {
                    servicingInfo.TotalMissions = (servicingInfo.TotalMissions.HasValue ? servicingInfo.TotalMissions + 1 : 1);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateSolo(Machine machine, DataLayerContext dataContext)
        {
            _ = machine ?? throw new System.ArgumentNullException(nameof(machine));
            if (dataContext is null)
            {
                dataContext = this.dataContext;
            }
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(ElevatorDataProvider.GetAxesCacheKey());
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
        }

        public void UpdateTotalAutomaticTime(TimeSpan duration)
        {
            lock (this.dataContext)
            {
                var machineStat = this.dataContext.MachineStatistics.LastOrDefault();
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
                var machineStat = this.dataContext.MachineStatistics.LastOrDefault();
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
        /// <param name="distance">space in millimeters</param>
        public void UpdateVerticalAxisStatistics(double distance)
        {
            lock (this.dataContext)
            {
                var machineStat = this.dataContext.MachineStatistics.LastOrDefault();
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
            var machineStat = dataContext.MachineStatistics.LastOrDefault();
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
        /// <param name="primaryConnectionString"></param>
        /// <returns>
        ///     The path
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

        #endregion
    }
}
