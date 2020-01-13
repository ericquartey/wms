using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MissionsDataProvider : IMissionsDataProvider
    {
        #region Fields

        private static Dictionary<int, Mission> missionCache = new Dictionary<int, Mission>();

        private readonly DataLayerContext dataContext;

        private readonly IErrorsProvider errorProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<DataLayerService> logger;

        #endregion

        #region Constructors

        public MissionsDataProvider(
            DataLayerContext dataContext,
            IErrorsProvider errorProvider,
            IEventAggregator eventAggregator,
            ILogger<DataLayerService> logger)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.errorProvider = errorProvider ?? throw new ArgumentNullException(nameof(errorProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public bool CanCreateMission(int loadingUnitId, BayNumber targetBay)
        {
            var returnValue = true;
            lock (missionCache)
            {
                // no duplicate of LU
                returnValue = !missionCache.Any(m => m.Value.LoadUnitId == loadingUnitId
                    && m.Value.Status == MissionStatus.Executing
                    );
                if (!returnValue)
                {
                    this.errorProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisLoadUnit, targetBay);
                }
                else
                {
                    // no duplicate of targetBay
                    returnValue = !missionCache.Any(m => m.Value.TargetBay == targetBay
                        && m.Value.Status == MissionStatus.Executing
                        );
                    if (!returnValue)
                    {
                        this.errorProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisBay, targetBay);
                    }
                }
            }

            return returnValue;
        }

        public Mission Complete(int id)
        {
            lock (missionCache)
            {
                var mission = missionCache[id];
                if (mission is null)
                {
                    throw new EntityNotFoundException(nameof(mission));
                }

                this.logger.LogInformation("Completing mission {id}", id);

                mission.Status = MissionStatus.Completed;

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

        public Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber)
        {
            lock (missionCache)
            {
                var entry = this.dataContext.Missions
                    .Add(
                    new Mission
                    {
                        CreationDate = DateTime.Now,
                        LoadUnitId = loadingUnitId,
                        TargetBay = bayNumber,
                        MissionType = MissionType.OUT,
                        Status = MissionStatus.New
                    });

                this.dataContext.SaveChanges();

                UpdateCache(entry.Entity);

                this.logger.LogInformation("Created internal MAS bay mission.");

                return entry.Entity;
            }
        }

        public Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, int wmsId, int wmsPriority)
        {
            lock (missionCache)
            {
                var transaction = this.dataContext.Database.BeginTransaction();

                if (missionCache.Any(m =>
                    m.Value.Status != MissionStatus.Completed
                    &&
                    m.Value.Status != MissionStatus.Aborted
                    &&
                    m.Value.WmsId == wmsId))
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

                UpdateCache(entry.Entity);

                this.logger.LogInformation($"Created MAS bay mission from WMS mission id={wmsId}");

                return entry.Entity;
            }
        }

        public Mission CreateRecallMission(int loadingUnitId, BayNumber bayNumber)
        {
            lock (missionCache)
            {
                var entry = this.dataContext.Missions.Add(
                    new Mission
                    {
                        CreationDate = DateTime.Now,
                        LoadUnitId = loadingUnitId,
                        TargetBay = bayNumber,
                        Status = MissionStatus.New,
                        MissionType = MissionType.IN
                    });

                this.dataContext.SaveChanges();

                UpdateCache(entry.Entity);

                this.logger.LogInformation("Created internal MAS recall mission.");

                return entry.Entity;
            }
        }

        public void Delete(int id)
        {
            lock (missionCache)
            {
                var mission = this.dataContext.Missions.SingleOrDefault(m => m.Id == id);
                if (mission is null)
                {
                    throw new EntityNotFoundException(nameof(mission));
                }

                this.dataContext.Missions.Remove(mission);

                this.dataContext.SaveChanges();

                if (missionCache.ContainsKey(mission.Id))
                {
                    missionCache.Remove(mission.Id);
                }

                this.logger.LogInformation($"Deleted MAS mission {mission.Id}.");
            }
        }

        public IEnumerable<Mission> GetAllActiveMissions()
        {
            lock (missionCache)
            {
                return missionCache
                    .Where(x => x.Value.Status != MissionStatus.Completed && x.Value.Status != MissionStatus.Aborted)
                    .OrderBy(o => o.Value.Priority)
                    .ThenBy(o => o.Value.CreationDate)
                    .Select(s => s.Value);
            }
        }

        public IEnumerable<Mission> GetAllActiveMissionsByBay(BayNumber bayNumber)
        {
            lock (missionCache)
            {
                return missionCache
                    .Where(x => x.Value.TargetBay == bayNumber
                        && x.Value.Status != MissionStatus.Completed && x.Value.Status != MissionStatus.Aborted)
                    .OrderBy(o => o.Value.Priority)
                    .ThenBy(o => o.Value.CreationDate)
                    .Select(s => s.Value);
            }
        }

        public IEnumerable<Mission> GetAllExecutingMissions(bool noCache = false)
        {
            lock (missionCache)
            {
                if (noCache)
                {
                    // reload cache from database
                    var missions = this.dataContext.Missions;
                    ResetMissionCache();
                    foreach (var mission in missions)
                    {
                        missionCache.Add(mission.Id, mission);
                    }
                    this.logger.LogTrace($"UpdateCache");
                }
                return missionCache
                    .Where(x => x.Value.Status == MissionStatus.Executing || x.Value.Status == MissionStatus.Waiting)
                    .Select(s => s.Value);
            }
        }

        public IEnumerable<Mission> GetAllWmsMissions()
        {
            lock (missionCache)
            {
                return missionCache
                    .Where(x => x.Value.WmsId != null)
                    .Select(s => s.Value);
            }
        }

        public Mission GetById(int id)
        {
            lock (missionCache)
            {
                var mission = missionCache.SingleOrDefault(m => m.Key == id).Value;
                if (mission is null)
                {
                    throw new EntityNotFoundException(nameof(mission));
                }
                return mission;
            }
        }

        public bool IsMissionInWaitState(BayNumber bayNumber, int loadingUnitId)
        {
            lock (missionCache)
            {
                return missionCache.Any(m => m.Value.TargetBay == bayNumber
                        && m.Value.Status == MissionStatus.Waiting
                        && m.Value.LoadUnitId == loadingUnitId);
            }
        }

        public void ResetMachine(MessageActor sender)
        {
            var messageData = new MoveLoadingUnitMessageData(
                        MissionType.NoType,
                        LoadingUnitLocation.NoLocation,
                        LoadingUnitLocation.NoLocation,
                        null,
                        null,
                        null,
                        false,
                        false,
                        null,
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

            lock (missionCache)
            {
                this.dataContext.Missions.Update(mission);

                this.dataContext.SaveChanges();

                UpdateCache(mission);
                this.logger.LogTrace($"UpdateCache");
            }
        }

        public void UpdateHomingMissions(BayNumber bayNumber, Axis axis)
        {
            lock (missionCache)
            {
                if (axis == Axis.HorizontalAndVertical)
                {
                    axis = Axis.Horizontal;
                }
                var missions = missionCache.Where(m => m.Value.NeedHomingAxis == axis
                        && (bayNumber == BayNumber.ElevatorBay || m.Value.TargetBay == bayNumber)
                        )
                    .Select(s => s.Value);
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

        private static void ResetMissionCache()
        {
            missionCache = new Dictionary<int, Mission>();
        }

        private static void UpdateCache(Mission mission)
        {
            lock (missionCache)
            {
                if (missionCache.ContainsKey(mission.Id))
                {
                    missionCache[mission.Id] = mission;
                }
                else
                {
                    missionCache.Add(mission.Id, mission);
                }
            }
        }

        #endregion
    }
}
