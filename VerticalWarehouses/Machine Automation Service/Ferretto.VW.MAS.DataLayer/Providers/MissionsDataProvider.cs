using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MissionsDataProvider : IMissionsDataProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly DataLayerContext dataContext;

        private readonly IErrorsProvider errorProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<DataLayerService> logger;

        private readonly IMachineProvider machineProvider;

        #endregion

        #region Constructors

        public MissionsDataProvider(
            DataLayerContext dataContext,
            IErrorsProvider errorProvider,
            IBaysDataProvider baysDataProvider,
            IEventAggregator eventAggregator,
            ILogger<DataLayerService> logger,
            IMachineProvider machineProvider)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.errorProvider = errorProvider ?? throw new ArgumentNullException(nameof(errorProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
        }

        #endregion

        #region Methods

        public bool CanCreateMission(int loadingUnitId, BayNumber targetBay)
        {
            var returnValue = true;
            lock (this.dataContext)
            {
                // no duplicate of LU
                returnValue = !this.dataContext.Missions.Any(m => m.LoadUnitId == loadingUnitId
                    && (m.Status == MissionStatus.Executing
                        || (m.Status == MissionStatus.New && m.MissionType != MissionType.WMS && m.MissionType != MissionType.OUT)
                        )
                    );
                if (!returnValue)
                {
                    this.errorProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisLoadUnit, targetBay, $"{ErrorDescriptions.LoadUnitNumber} {loadingUnitId}");
                }
                else
                {
                    // no duplicate of targetBay
                    returnValue = !this.dataContext.Missions.Any(m => m.TargetBay == targetBay
                        && m.Status == MissionStatus.Executing
                        );
                    if (!returnValue)
                    {
                        this.errorProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisBay, targetBay);
                    }
                }
            }

            return returnValue;
        }

        public void CheckPendingChanges()
        {
            if (this.dataContext.ChangeTracker.HasChanges())
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    var entities = this.dataContext.ChangeTracker.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted).ToList();
                    foreach (var entry in entities)
                    {
                        this.logger.LogError($"Entity {entry.Entity.GetType().Name} was not saved");

                        var properties = entry.Properties.Where(x => x.IsModified).ToList();

                        foreach (var property in properties)
                        {
                            this.logger.LogError($"Property {property.Metadata.Name} is modified Current Value: '{property.CurrentValue}' Original value: '{property.OriginalValue}'");
                        }
                    }

                    System.Diagnostics.Debugger.Break();
                }

                //throw new Exception("Missing SaveChanges");
            }
        }

        public Mission Complete(int id)
        {
            lock (this.dataContext)
            {
                var mission = this.dataContext.Missions.SingleOrDefault(m => m.Id == id);
                if (mission is null)
                {
                    throw new EntityNotFoundException(nameof(mission));
                }

                this.logger.LogInformation("Completing mission {id}", id);
                if (mission.Status != MissionStatus.Completed)
                {
                    mission.Status = MissionStatus.Completed;
                    mission.MissionTime = mission.MissionTime.Add(DateTime.UtcNow - mission.StepTime);
                    this.machineProvider.UpdateMissionTime(mission.MissionTime);
                }

                if (this.baysDataProvider.IsMissionInBay(mission))
                {
                    this.baysDataProvider.ClearMission(mission.TargetBay);
                }

                if (mission.WmsId is null)
                {
                    this.Delete(mission.Id);
                }
                else
                {
                    this.Update(mission);
                }

                return mission;
            }
        }

        public Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, MissionType missionType, LoadingUnitLocation destination = LoadingUnitLocation.NoLocation)
        {
            lock (this.dataContext)
            {
                if (this.dataContext.Missions.Any(m => m.LoadUnitId == loadingUnitId
                         && m.TargetBay == bayNumber
                         && ((m.Status == MissionStatus.New && m.MissionType != MissionType.WMS && m.MissionType != MissionType.OUT)
                            || m.Status == MissionStatus.Executing
                            || (missionType == MissionType.OUT && m.Status == MissionStatus.Waiting))
                        )
                    )
                {
                    this.errorProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisLoadUnit, bayNumber, $"{ErrorDescriptions.LoadUnitNumber} {loadingUnitId}");
                    throw new InvalidOperationException(string.Format(Resources.Missions.ResourceManager.GetString("ActiveMissionForLoadingUnit", CommonUtils.Culture.Actual), loadingUnitId, (int)bayNumber));
                }
                var entry = this.dataContext.Missions
                    .Add(
                    new Mission
                    {
                        CreationDate = DateTime.UtcNow,
                        StepTime = DateTime.UtcNow,
                        LoadUnitId = loadingUnitId,
                        TargetBay = bayNumber,
                        MissionType = missionType,
                        LoadUnitDestination = destination,
                        Status = MissionStatus.New
                    });

                this.dataContext.SaveChanges();

                this.logger.LogInformation($"Created internal MAS bay mission for load unit {loadingUnitId}.");

                return entry.Entity;
            }
        }

        public Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, int wmsId, int wmsPriority, LoadingUnitLocation destination = LoadingUnitLocation.NoLocation)
        {
            lock (this.dataContext)
            {
                var transaction = this.dataContext.Database.BeginTransaction();

                if (this.dataContext.Missions.Any(m =>
                    m.Status != MissionStatus.Completed
                    && m.Status != MissionStatus.Aborted
                    && m.WmsId == wmsId)
                    )
                {
                    transaction.Rollback();
                    throw new InvalidOperationException(string.Format(Resources.Missions.ResourceManager.GetString("ActiveMissionForWMS", CommonUtils.Culture.Actual), wmsId));
                }

                var entry = this.dataContext.Missions.Add(
                    new Mission
                    {
                        CreationDate = DateTime.UtcNow,
                        StepTime = DateTime.UtcNow,
                        LoadUnitId = loadingUnitId,
                        LoadUnitSource = LoadingUnitLocation.LoadUnit,
                        MissionType = MissionType.WMS,
                        Priority = wmsPriority,
                        Status = MissionStatus.New,
                        TargetBay = bayNumber,
                        WmsId = wmsId,
                        LoadUnitDestination = destination
                    });

                this.dataContext.SaveChanges();

                transaction.Commit();

                this.logger.LogInformation($"Created MAS bay mission from WMS mission id={wmsId} for load unit {loadingUnitId}");

                return entry.Entity;
            }
        }

        public Mission CreateRecallMission(int loadingUnitId, BayNumber bayNumber, MissionType missionType)
        {
            lock (this.dataContext)
            {
                var existingMission = this.dataContext.Missions
                    .AsNoTracking()
                    .SingleOrDefault(m =>
                        m.LoadUnitId == loadingUnitId
                        && m.MissionType == missionType
                        && m.Status != MissionStatus.Completed
                        && m.Status != MissionStatus.Aborted
                        );

                if (existingMission != null)
                {
                    return existingMission;
                }

                var entry = this.dataContext.Missions.Add(
                    new Mission
                    {
                        CreationDate = DateTime.UtcNow,
                        StepTime = DateTime.UtcNow,
                        LoadUnitId = loadingUnitId,
                        TargetBay = bayNumber,
                        Status = MissionStatus.New,
                        LoadUnitDestination = LoadingUnitLocation.Cell,
                        MissionType = missionType
                    });

                this.dataContext.SaveChanges();

                this.logger.LogInformation($"Created internal MAS recall mission for load unit {loadingUnitId}.");

                return entry.Entity;
            }
        }

        public void Delete(int id)
        {
            lock (this.dataContext)
            {
                var mission = this.dataContext.Missions.SingleOrDefault(m => m.Id == id);
                if (mission is null)
                {
                    throw new EntityNotFoundException(nameof(mission));
                }

                this.dataContext.Missions.Remove(mission);

                this.dataContext.SaveChanges();

                this.logger.LogInformation($"Deleted MAS mission {mission.Id}.");
            }
        }

        public IEnumerable<Mission> GetAllActiveMissions()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions
                    .AsNoTracking()
                    .Where(x => x.Status != MissionStatus.Completed
                        && x.Status != MissionStatus.Aborted)
                    .OrderBy(o => o.Priority)
                    .ThenBy(o => o.CreationDate)
                    .ToList();
            }
        }

        public IEnumerable<Mission> GetAllActiveMissionsByBay(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions
                    .Where(x => x.TargetBay == bayNumber
                        && x.Status != MissionStatus.Completed
                        && x.Status != MissionStatus.Aborted)
                    .OrderBy(o => o.Priority)
                    .ThenBy(o => o.CreationDate)
                    .ToList();
            }
        }

        public IEnumerable<Mission> GetAllExecutingMissions()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions
                    .Where(x => x.Status == MissionStatus.Executing
                        || x.Status == MissionStatus.Waiting)
                    .ToList();
            }
        }

        public IEnumerable<Mission> GetAllMissions()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions
                    .AsNoTracking()
                    .OrderBy(o => o.Priority)
                    .ThenBy(o => o.CreationDate)
                    .ToList();
            }
        }

        public IEnumerable<Mission> GetAllWmsMissions()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions
                    .Where(x => x.WmsId != null
                        && x.Status != MissionStatus.Completed
                        && x.Status != MissionStatus.Aborted)
                    .ToList();
            }
        }

        public Mission GetById(int id)
        {
            lock (this.dataContext)
            {
                var mission = this.dataContext.Missions.SingleOrDefault(m => m.Id == id);
                if (mission is null)
                {
                    throw new EntityNotFoundException(nameof(mission));
                }
                return mission;
            }
        }

        public Mission GetByWmsId(int id)
        {
            lock (this.dataContext)
            {
                var mission = this.dataContext.Missions.SingleOrDefault(m => m.WmsId == id
                    && m.Status != MissionStatus.Completed
                    && m.Status != MissionStatus.Aborted);
                if (mission is null)
                {
                    throw new EntityNotFoundException(nameof(mission));
                }
                return mission;
            }
        }

        public IDbContextTransaction GetContextTransaction()
        {
            return this.dataContext.Database.BeginTransaction();
        }

        public bool IsEnabeNoteRules()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.EnabeNoteRules).First();
            }
        }

        public bool IsLocalMachineItems()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsLocalMachineItems).First();
            }
        }

        public bool IsMissionInWaitState(BayNumber bayNumber, int loadingUnitId)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions.Any(m => m.TargetBay == bayNumber
                        && m.Status == MissionStatus.Waiting
                        && m.LoadUnitId == loadingUnitId);
            }
        }

        public bool IsOrderList()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.AsNoTracking().Select(m => m.IsOrderList).First();
            }
        }

        // removes completed missions older than 1 day
        public int PurgeMissions()
        {
            lock (this.dataContext)
            {
                var count = 0;
                foreach (var mission in this.dataContext.Missions
                    .Where(x => DateTime.UtcNow.Subtract(x.CreationDate).Days > 1
                        && (x.Status == MissionStatus.Completed
                            || x.Status == MissionStatus.Aborted)
                        )
                    .ToList()
                    )
                {
                    this.dataContext.Missions.Remove(mission);

                    var bays = this.dataContext.Bays.Include(b => b.CurrentMission).Where(b => b.CurrentMission.Id == mission.Id);
                    if (bays.Any())
                    {
                        foreach (var bay in bays)
                        {
                            bay.CurrentMission = null;
                            this.dataContext.Bays.Update(bay);
                            this.logger.LogDebug($"Cleared bay mission {mission.Id}, Bay {bay.Number}.");
                        }
                    }

                    this.logger.LogInformation($"Deleted MAS mission {mission.Id}, Wms Id {mission.WmsId}.");
                    count++;
                }
                if (count > 0)
                {
                    this.dataContext.SaveChanges();
                }
                return count;
            }
        }

        public void Reload(Mission mission)
        {
            this.dataContext.Entry(mission).Reload();
        }

        public void ResetMachine(MessageActor sender)
        {
            var messageData = new MoveLoadingUnitMessageData(
                        MissionType.NoType,
                        LoadingUnitLocation.NoLocation,
                        LoadingUnitLocation.NoLocation,
                        sourceCellId: null,
                        destinationCellId: null,
                        loadUnitId: null,
                        insertLoadUnit: false,
                        missionId: null,
                        loadUnitHeight: null,
                        netWeight: null,
                        CommandAction.Abort);

            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
                    new CommandMessage(
                        messageData,
                        $"Reset mission by ResetMachine",
                        MessageActor.MachineManager,
                        sender,
                        MessageType.MoveLoadingUnit,
                        BayNumber.BayOne,
                        BayNumber.BayOne));
        }

        public void Update(Mission mission)
        {
            if (mission is null)
            {
                throw new ArgumentNullException(nameof(mission));
            }

            lock (this.dataContext)
            {
                this.dataContext.AddOrUpdate(mission, (e) => e.Id);

                this.dataContext.SaveChanges();

                this.CheckPendingChanges();
            }
        }

        #endregion
    }
}
