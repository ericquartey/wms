using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class LoadingUnitsDataProvider : ILoadingUnitsDataProvider
    {
        #region Fields

        /// <summary>
        /// TODO Consider transformomg this constant in configuration parameters.
        /// </summary>
        private const double MinimumLoadOnBoard = 10.0;

        private const string ROTATION_CLASS_A = "A";

        private const string ROTATION_CLASS_B = "B";

        private const string ROTATION_CLASS_C = "C";

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly DataLayerContext dataContext;

        private readonly IErrorsProvider errorsProvider;

        private readonly WMS.Data.WebAPI.Contracts.ILoadingUnitsWmsWebService loadingUnitsWmsWebService;

        private readonly ILogger<LoadingUnitsDataProvider> logger;

        private readonly IMachineProvider machineProvider;

        private readonly NotificationEvent notificationEvent;

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        #endregion

        #region Constructors

        public LoadingUnitsDataProvider(
            DataLayerContext dataContext,
            IMachineProvider machineProvider,
            ICellsProvider cellsProvider,
            IBaysDataProvider baysDataProvider,
            IErrorsProvider errorsProvider,
            IWmsSettingsProvider wmsSettingsProvider,
            WMS.Data.WebAPI.Contracts.ILoadingUnitsWmsWebService loadingUnitsWmsWebService,
            ILogger<LoadingUnitsDataProvider> logger,
            IEventAggregator eventAggregator)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.wmsSettingsProvider = wmsSettingsProvider ?? throw new ArgumentNullException(nameof(wmsSettingsProvider));
            this.loadingUnitsWmsWebService = loadingUnitsWmsWebService ?? throw new ArgumentNullException(nameof(loadingUnitsWmsWebService));
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
        }

        #endregion

        #region Methods

        public void Add(IEnumerable<LoadingUnit> loadingUnits)
        {
            lock (this.dataContext)
            {
                this.dataContext.LoadingUnits.AddRange(loadingUnits);

                this.dataContext.SaveChanges();
            }
        }

        public void AddTestUnit(LoadingUnit testUnit)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == testUnit.Id);

                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(testUnit.Id);
                }
                loadingUnit.IsInFullTest = true;

                this.dataContext.SaveChanges();
            }
        }

        public MachineErrorCode CheckWeight(int id)
        {
            var check = MachineErrorCode.NoError;
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == id);

                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(id);
                }

                var machine = this.machineProvider.GetMinMaxHeight();

                if (loadingUnit.GrossWeight < MinimumLoadOnBoard)
                {
                    check = MachineErrorCode.LoadUnitWeightTooLow;
                }
                else if (loadingUnit.GrossWeight > loadingUnit.MaxNetWeight + loadingUnit.Tare + 20)
                {
                    check = MachineErrorCode.LoadUnitWeightExceeded;
                }
                else
                {
                    var totalWeight = this.dataContext
                        .LoadingUnits
                        .ToList()
                        .Where(lu => lu.IsIntoMachineOrBlocked)
                        .Sum(lu => lu.GrossWeight);
                    if (loadingUnit.GrossWeight + totalWeight > machine.MaxGrossWeight)
                    {
                        check = MachineErrorCode.MachineWeightExceeded;
                    }
                }
            }
            return check;
        }

        public int CountIntoMachine()
        {
            lock (this.dataContext)
            {
                return this.dataContext.LoadingUnits.AsNoTracking().ToArray()
                    .Count(l => l.IsIntoMachineOrBlocked);
            }
        }

        public IEnumerable<LoadingUnit> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.LoadingUnits
                    .AsNoTracking()
                    .Include(l => l.Cell)
                    .ThenInclude(c => c.Panel)
                    .ToArray();
            }
        }

        public IEnumerable<LoadingUnit> GetAllCompacting()
        {
            lock (this.dataContext)
            {
                return this.dataContext.LoadingUnits
                    .AsNoTracking()
                    .Include(l => l.Cell)
                    .ThenInclude(c => c.Panel)
                    .Where(x => x.Cell != null
                        && !this.dataContext.Missions.Any(m => m.LoadUnitId == x.Id
                            && (m.Status == MissionStatus.Executing
                                || (m.Status == MissionStatus.New && m.MissionType != MissionType.WMS && m.MissionType != MissionType.OUT)
                                )
                            )
                        )
                    .ToArray();
            }
        }

        public IEnumerable<LoadingUnit> GetAllNotTestUnits()
        {
            lock (this.dataContext)
            {
                return this.dataContext.LoadingUnits
                    .AsNoTracking()
                    .Where(u => u.IsInFullTest == false)
                    .Include(l => l.Cell)
                    .ThenInclude(c => c.Panel)
                    .ToArray();
            }
        }

        public IEnumerable<LoadingUnit> GetAllTestUnits()
        {
            lock (this.dataContext)
            {
                return this.dataContext.LoadingUnits
                    .AsNoTracking()
                    .Where(u => u.IsInFullTest)
                    .Include(l => l.Cell)
                    .ThenInclude(c => c.Panel)
                    .ToArray();
            }
        }

        public LoadingUnit GetByBayId(int bayId)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext.BayPositions.Include(i => i.LoadingUnit).AsNoTracking()
                    .FirstOrDefault().LoadingUnit;
                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(bayId);
                }

                return loadingUnit;
            }
        }

        public LoadingUnit GetByCellId(int cellId)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext.LoadingUnits.AsNoTracking()
                    .FirstOrDefault(l => l.CellId == cellId);
                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(cellId);
                }

                return loadingUnit;
            }
        }

        public LoadingUnit GetById(int id)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext.LoadingUnits.AsNoTracking()
                    .FirstOrDefault(l => l.Id == id);
                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(id);
                }

                return loadingUnit;
            }
        }

        public LoadingUnit GetCellById(int id)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext.LoadingUnits
                .Include(l => l.Cell)
                .ThenInclude(l => l.Panel)
                .SingleOrDefault(p => p.Id.Equals(id));

                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(id);
                }

                return loadingUnit;
            }
        }

        public double GetLoadUnitMaxHeight()
        {
            lock (this.dataContext)
            {
                var loadUnitMaxHeight = this.dataContext.Machines.AsNoTracking().Select(s => s.LoadUnitMaxHeight).Single();

                return loadUnitMaxHeight;
            }
        }

        public IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics()
        {
            lock (this.dataContext)
            {
                var loadingUnits = this.dataContext
                    .LoadingUnits
                    .Select(l =>
                        new LoadingUnitSpaceStatistics
                        {
                            MissionsCount = l.MissionsCount,
                            Code = l.Code,
                        }).ToArray();

                return loadingUnits;
            }
        }

        public IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics()
        {
            lock (this.dataContext)
            {
                var loadingUnits = this.dataContext.LoadingUnits
                .Select(l =>
                     new LoadingUnitWeightStatistics
                     {
                         Height = l.Height,
                         GrossWeight = l.GrossWeight,
                         Tare = l.Tare,
                         Code = l.Code,
                         MaxNetWeight = l.MaxNetWeight,
                         MaxWeightPercentage = (l.GrossWeight - l.Tare) * 100 / l.MaxNetWeight,
                     })
                .ToArray();

                return loadingUnits;
            }
        }

        public void Import(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext context)
        {
            _ = loadingUnits ?? throw new ArgumentNullException(nameof(loadingUnits));

            context.Delete(loadingUnits, (e) => e.Id);
            loadingUnits.ForEach((l) => context.AddOrUpdate(l, (e) => e.Id));

            this.machineProvider.UpdateWeightStatistics(context);
        }

        public void Insert(int loadingUnitsId)
        {
            var machine = this.machineProvider.GetMinMaxHeight();
            lock (this.dataContext)
            {
                var loadingUnits = new LoadingUnit
                {
                    Id = loadingUnitsId,
                    Tare = machine.LoadUnitTare,
                    MaxNetWeight = machine.LoadUnitMaxNetWeight,
                    Height = 0,
                    RotationClass = ROTATION_CLASS_A
                };

                this.dataContext.LoadingUnits.Add(loadingUnits);

                this.dataContext.SaveChanges();
            }
        }

        public void Remove(int loadingUnitsId)
        {
            var lu = this.dataContext.LoadingUnits.SingleOrDefault(p => p.Id.Equals(loadingUnitsId));
            if (lu is null)
            {
                throw new EntityNotFoundException($"LoadingUnit ID={loadingUnitsId}");
            }

            if (lu.CellId.HasValue)
            {
                this.cellsProvider.SetLoadingUnit(lu.CellId.Value, null);
            }
            if (lu.Status == LoadingUnitStatus.InBay)
            {
                this.baysDataProvider.RemoveLoadingUnit(loadingUnitsId);
            }
            else
            {
                lock (this.dataContext)
                {
                    this.dataContext.LoadingUnits.Remove(lu);
                    this.dataContext.SaveChanges();
                    this.baysDataProvider.NotifyRemoveLoadUnit(loadingUnitsId, LoadingUnitLocation.NoLocation);
                }
            }
        }

        public void RemoveTestUnit(LoadingUnit testUnit)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == testUnit.Id);

                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(testUnit.Id);
                }
                loadingUnit.IsInFullTest = false;

                this.dataContext.SaveChanges();
            }
        }

        public async Task SaveAsync(LoadingUnit loadingUnit)
        {
            int? cellIdOld = null;
            double luHeightOld = 0;
            var originalStatus = loadingUnit.Status;

            var luDb = this.dataContext.LoadingUnits.SingleOrDefault(p => p.Id.Equals(loadingUnit.Id));
            if (luDb is null)
            {
                throw new EntityNotFoundException($"Load Unit ID={loadingUnit.Id}");
            }

            if (loadingUnit.CellId.HasValue &&
                loadingUnit.Height <= 0)
            {
                throw new ArgumentException($"Load Unit with Height to 0 (Id={loadingUnit.Id})");
            }

            if (loadingUnit.GrossWeight < 0)
            {
                throw new ArgumentException($"Load Unit with negative Weight (Id={loadingUnit.Id})");
            }

            if (loadingUnit.GrossWeight > loadingUnit.MaxNetWeight + loadingUnit.Tare + 100)
            {
                throw new ArgumentException($"Load Unit with Overload (Id={loadingUnit.Id})");
            }

            luHeightOld = luDb.Height;
            if (luDb.CellId.HasValue &&
                (luDb.CellId != loadingUnit.CellId ||
                luDb.Height != loadingUnit.Height ||
                loadingUnit.Status == LoadingUnitStatus.InElevator))
            {
                cellIdOld = luDb.CellId.Value;
                this.cellsProvider.SetLoadingUnit(luDb.CellId.Value, null);

                lock (this.dataContext)
                {
                    luDb.Height = loadingUnit.Height;
                    this.dataContext.SaveChanges();
                }
            }

            if (loadingUnit.CellId.HasValue &&
                (luDb.CellId != loadingUnit.CellId ||
                 luDb.Height != loadingUnit.Height))
            {
                if (luDb.Height != loadingUnit.Height)
                {
                    lock (this.dataContext)
                    {
                        luDb.Height = loadingUnit.Height;
                        this.dataContext.SaveChanges();
                    }
                }

                if (!this.cellsProvider.CanFitLoadingUnit(loadingUnit.CellId.Value, loadingUnit.Id))
                {
                    lock (this.dataContext)
                    {
                        luDb.Height = luHeightOld;
                        this.dataContext.SaveChanges();
                    }

                    if (cellIdOld.HasValue)
                    {
                        this.cellsProvider.SetLoadingUnit(cellIdOld.Value, luDb.Id);
                    }

                    throw new ArgumentException($"LoadingUnit error cell or height (CellId={loadingUnit.CellId}, Id={loadingUnit.Id})");
                }
            }

            lock (this.dataContext)
            {
                luDb.Description = loadingUnit.Description;
                luDb.MissionsCount = loadingUnit.MissionsCount;
                luDb.GrossWeight = loadingUnit.GrossWeight;
                luDb.MaxNetWeight = loadingUnit.MaxNetWeight;
                luDb.Tare = loadingUnit.Tare;
                luDb.Height = loadingUnit.Height;
                luDb.LaserOffset = loadingUnit.LaserOffset;
                luDb.MissionsCountRotation = loadingUnit.MissionsCountRotation;
                luDb.IsRotationClassFixed = loadingUnit.IsRotationClassFixed;
                if (loadingUnit.IsRotationClassFixed)
                {
                    luDb.RotationClass = loadingUnit.RotationClass;
                }

                if (originalStatus != LoadingUnitStatus.InElevator
                    && loadingUnit.Status != LoadingUnitStatus.Undefined
                    )
                {
                    luDb.Status = loadingUnit.Status;
                }
                else if (luDb.CellId != null)
                {
                    luDb.Status = LoadingUnitStatus.InLocation;
                }

                this.dataContext.SaveChanges();
            }

            if (loadingUnit.CellId.HasValue &&
                luDb.CellId != loadingUnit.CellId &&
                originalStatus != LoadingUnitStatus.InElevator)
            {
                this.cellsProvider.SetLoadingUnit(loadingUnit.CellId.Value, loadingUnit.Id);
            }

            this.notificationEvent.Publish(
                    new NotificationMessage
                    {
                        Description = $"{loadingUnit.Id}",
                        Destination = MessageActor.MissionManager,
                        Source = MessageActor.WebApi,
                        Type = MessageType.SaveToWms,
                        RequestingBay = BayNumber.None,
                    });
        }

        public async Task SaveToWmsAsync(int loadingUnitId)
        {
            if (this.wmsSettingsProvider.IsEnabled)
            {
                var loadUnitDetail = new WMS.Data.WebAPI.Contracts.LoadingUnitDetails();
                var loadingUnit = this.GetCellById(loadingUnitId);
                var machineId = this.machineProvider.GetIdentity();
                loadUnitDetail.CellId = loadingUnit.CellId;
                loadUnitDetail.CellSide = (loadingUnit.CellId.HasValue && loadingUnit.Cell.Side != WarehouseSide.NotSpecified)
                    ? (loadingUnit.Cell.Side == WarehouseSide.Front ? WMS.Data.WebAPI.Contracts.Side.Front : WMS.Data.WebAPI.Contracts.Side.Back)
                    : WMS.Data.WebAPI.Contracts.Side.NotSpecified;
                loadUnitDetail.Code = loadingUnit.Code;
                loadUnitDetail.CreationDate = DateTimeOffset.UtcNow;
                loadUnitDetail.LastModificationDate = DateTimeOffset.UtcNow;
                loadUnitDetail.EmptyWeight = loadingUnit.Tare;
                loadUnitDetail.Height = loadingUnit.Height;
                loadUnitDetail.Id = loadingUnit.Id;
                loadUnitDetail.MachineId = machineId;
                loadUnitDetail.MissionsCount = loadingUnit.MissionsCount;
                loadUnitDetail.Note = loadingUnit.Description;
                loadUnitDetail.Weight = (int)loadingUnit.GrossWeight;

                try
                {
                    this.logger.LogInformation($"Save load unit {loadingUnit.Id} to wms ");
                    await this.loadingUnitsWmsWebService.SaveAsync(loadingUnit.Id, loadUnitDetail);
                }
                catch (Exception ex)
                {
                    this.errorsProvider.RecordNew(MachineErrorCode.WmsError, BayNumber.None, ex.Message.Replace("\n", " ").Replace("\r", " "));
                }
            }
        }

        public void SetHeight(int id, double height)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == id);

                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(id);
                }
                if (height == 0)
                {
                    loadingUnit.Height = 0;
                }
                else
                {
                    loadingUnit.Height = Math.Max(loadingUnit.Height, height);
                }
                this.dataContext.SaveChanges();
            }
        }

        public void SetLaserOffset(int id, double laserOffset)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == id);

                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException($"Load Unit ID={id}");
                }
                loadingUnit.LaserOffset = laserOffset;

                this.dataContext.SaveChanges();
            }
        }

        public void SetMissionCountRotation(int id, MissionType missionType)
        {
            lock (this.dataContext)
            {
                if (missionType == MissionType.OUT || missionType == MissionType.WMS)
                {
                    var loadUnit = this.dataContext.LoadingUnits.FirstOrDefault(l => l.Id == id);
                    if (loadUnit != null)
                    {
                        loadUnit.MissionsCountRotation++;
                        this.dataContext.SaveChanges();
                    }
                }
            }
        }

        public bool SetRotationClass()
        {
            var retval = false;
            lock (this.dataContext)
            {
                var loadUnits = this.dataContext
                    .LoadingUnits
                    .Where(l => l.Status != LoadingUnitStatus.Blocked && !l.IsRotationClassFixed);

                var totalMissionCount = loadUnits.Sum(s => s.MissionsCountRotation);
                var missionCount = 0;

                if (totalMissionCount > 1)
                {
                    foreach (var loadUnit in loadUnits.OrderByDescending(o => o.MissionsCountRotation))
                    {
                        if (missionCount <= totalMissionCount * 0.8)
                        {
                            loadUnit.RotationClass = ROTATION_CLASS_A;
                        }
                        else if (missionCount <= totalMissionCount * 0.95)
                        {
                            loadUnit.RotationClass = ROTATION_CLASS_B;
                        }
                        else
                        {
                            loadUnit.RotationClass = ROTATION_CLASS_C;
                        }
                        missionCount += loadUnit.MissionsCountRotation;
                        loadUnit.MissionsCountRotation = 0;
                    }

                    this.dataContext.SaveChanges();
                    retval = true;
                }
                else
                {
                    foreach (var loadUnit in loadUnits)
                    {
                        if (string.IsNullOrEmpty(loadUnit.RotationClass))
                        {
                            loadUnit.RotationClass = ROTATION_CLASS_A;
                        }
                    }

                    this.dataContext.SaveChanges();
                    retval = true;
                }
            }
            return retval;
        }

        public void SetRotationClassFromUI(int id, string rotationClass)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == id);

                loadingUnit.RotationClass = rotationClass;

                this.dataContext.SaveChanges();
            }
        }

        public void SetStatus(int id, LoadingUnitStatus status)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == id);

                loadingUnit.Status = status;

                this.dataContext.SaveChanges();
            }
        }

        public void SetWeight(int id, double loadingUnitGrossWeight)
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext.Elevators
                    .Include(e => e.StructuralProperties)
                    .Single();

                var elevatorWeight = elevator.StructuralProperties.ElevatorWeight;

                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == id);

                if (loadingUnitGrossWeight < elevatorWeight)
                {
                    loadingUnit.GrossWeight = 0;
                }
                else if (loadingUnitGrossWeight > loadingUnit.MaxNetWeight + loadingUnit.Tare + elevatorWeight + 100)
                {
                    loadingUnit.GrossWeight = loadingUnit.MaxNetWeight + loadingUnit.Tare + 100;
                }
                else
                {
                    loadingUnit.GrossWeight = loadingUnitGrossWeight - elevatorWeight;
                }

                this.dataContext.SaveChanges();
            }
        }

        public void SetWeightFromUI(int id, double loadingUnitGrossWeight)
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext.Elevators
                    .Include(e => e.StructuralProperties)
                    .Single();

                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == id);

                if (loadingUnitGrossWeight <= loadingUnit.MaxNetWeight + loadingUnit.Tare
                    && loadingUnitGrossWeight >= loadingUnit.Tare)
                {
                    loadingUnit.GrossWeight = loadingUnitGrossWeight;
                }
                else
                {
                    this.logger.LogWarning($"SetWeight error for LoadUnit {id}! Do not change weight to {loadingUnitGrossWeight:0.000}");
                }

                this.dataContext.SaveChanges();
            }
        }

        public void TryAdd(int loadingUnitId)
        {
            var lu = this.dataContext.LoadingUnits.SingleOrDefault(p => p.Id.Equals(loadingUnitId));
            if (lu is null)
            {
                var machine = this.machineProvider.GetMinMaxHeight();
                lock (this.dataContext)
                {
                    var loadingUnits = new LoadingUnit
                    {
                        Id = loadingUnitId,
                        Tare = machine.LoadUnitTare,
                        MaxNetWeight = machine.LoadUnitMaxNetWeight,
                        Height = 0
                    };

                    this.dataContext.LoadingUnits.Add(loadingUnits);

                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateRange(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext dataContext)
        {
            _ = loadingUnits ?? throw new ArgumentNullException(nameof(loadingUnits));

            if (dataContext is null)
            {
                dataContext = this.dataContext;
            }

            loadingUnits.ForEach((l) => dataContext.AddOrUpdate(l, (e) => e.Id));

            dataContext.SaveChanges();
        }

        public void UpdateWeightStatistics()
        {
            lock (this.dataContext)
            {
                this.machineProvider.UpdateWeightStatistics(this.dataContext);
            }
        }

        #endregion
    }
}
