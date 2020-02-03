using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
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

        #endregion

        #region Constructors

        public MissionsDataProvider(
            DataLayerContext dataContext,
            IErrorsProvider errorProvider,
            IBaysDataProvider baysDataProvider,
            IEventAggregator eventAggregator,
            ILogger<DataLayerService> logger)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.errorProvider = errorProvider ?? throw new ArgumentNullException(nameof(errorProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                    && m.Status == MissionStatus.Executing
                    );
                if (!returnValue)
                {
                    this.errorProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisLoadUnit, targetBay);
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

                mission.Status = MissionStatus.Completed;

                if (mission.WmsId is null)
                {
                    if (this.baysDataProvider.IsMissionInBay(mission))
                    {
                        this.baysDataProvider.ClearMission(mission.TargetBay);
                    }
                    this.Delete(mission.Id);
                }
                else
                {
                    this.Update(mission);
                }

                return mission;
            }
        }

        public Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, MissionType missionType)
        {
            lock (this.dataContext)
            {
                var entry = this.dataContext.Missions
                    .Add(
                    new Mission
                    {
                        CreationDate = DateTime.Now,
                        LoadUnitId = loadingUnitId,
                        TargetBay = bayNumber,
                        MissionType = missionType,
                        Status = MissionStatus.New
                    });

                this.dataContext.SaveChanges();

                this.logger.LogInformation("Created internal MAS bay mission.");

                return entry.Entity;
            }
        }

        public Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, int wmsId, int wmsPriority)
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
                    throw new InvalidOperationException($"An active mission for WMS mission {wmsId} already exists.");
                }

                var entry = this.dataContext.Missions.Add(
                    new Mission
                    {
                        CreationDate = DateTime.Now,
                        LoadUnitId = loadingUnitId,
                        LoadUnitSource = LoadingUnitLocation.LoadUnit,
                        MissionType = MissionType.WMS,
                        Priority = wmsPriority,
                        Status = MissionStatus.New,
                        TargetBay = bayNumber,
                        WmsId = wmsId,
                    });

                this.dataContext.SaveChanges();

                transaction.Commit();

                this.logger.LogInformation($"Created MAS bay mission from WMS mission id={wmsId}");

                return entry.Entity;
            }
        }

        public Mission CreateRecallMission(int loadingUnitId, BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var entry = this.dataContext.Missions.Add(
                    new Mission
                    {
                        CreationDate = DateTime.Now,
                        LoadUnitId = loadingUnitId,
                        TargetBay = bayNumber,
                        Status = MissionStatus.New,
                        LoadUnitDestination = LoadingUnitLocation.Cell,
                        MissionType = MissionType.IN
                    });

                this.dataContext.SaveChanges();

                this.logger.LogInformation($"Created internal MAS recall mission for loading unit {loadingUnitId}.");

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
                    .Where(x => x.WmsId != null)
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

        public IDbContextTransaction GetContextTransaction()
        {
            return this.dataContext.Database.BeginTransaction();
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

        public void UpdateHomingMissions(BayNumber bayNumber, Axis axis)
        {
            lock (this.dataContext)
            {
                if (axis == Axis.HorizontalAndVertical)
                {
                    axis = Axis.Horizontal;
                }
                var missions = this.dataContext.Missions.Where(m => m.NeedHomingAxis == axis
                        && (bayNumber == BayNumber.ElevatorBay || m.TargetBay == bayNumber)
                        );
                if (missions.Any())
                {
                    foreach (var mission in missions)
                    {
                        mission.NeedHomingAxis = Axis.None;
                        this.Update(mission);
                        this.logger.LogDebug($"Elevator Homing executed for Load Unit {mission.LoadUnitId}");
                    }
                }
                else
                {
                    this.logger.LogDebug($"No Homing missions waiting for Bay {bayNumber}, axis {axis}");
                }
            }
        }

        #endregion
    }
}
