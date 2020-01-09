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
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MissionsDataProvider : IMissionsDataProvider
    {
        #region Fields

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
            // no duplicate of LU
            returnValue = !this.dataContext.Missions.Any(m => m.LoadingUnitId == loadingUnitId
                && m.Status == MissionStatus.Executing
                );
            if (!returnValue)
            {
                this.errorProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisLoadUnit);
            }
            else
            {
                // no duplicate of targetBay
                returnValue = !this.dataContext.Missions.Any(m => m.TargetBay == targetBay
                    && m.Status == MissionStatus.Executing
                    );
                if (!returnValue)
                {
                    this.errorProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisBay);
                }
            }

            return returnValue;
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
                    this.dataContext.Missions.Remove(mission);
                }

                this.dataContext.SaveChanges();

                return mission;
            }
        }

        public Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var entry = this.dataContext.Missions
                    .Add(
                    new Mission
                    {
                        FsmId = Guid.NewGuid(),
                        CreationDate = DateTime.Now,
                        LoadingUnitId = loadingUnitId,
                        TargetBay = bayNumber,
                        MissionType = MissionType.OUT,
                        Status = MissionStatus.New
                    })
                    ;

                this.dataContext.SaveChanges();

                this.logger.LogInformation("Created internal MAS bay mission.");

                return entry.Entity;
            }
        }

        public Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, int wmsId, int wmsPriority)
        {
            lock (this.dataContext)
            {
                var entry = this.dataContext.Missions.Add(
                    new Mission
                    {
                        FsmId = Guid.NewGuid(),
                        CreationDate = DateTime.Now,
                        LoadingUnitId = loadingUnitId,
                        LoadingUnitSource = LoadingUnitLocation.LoadingUnit,
                        MissionType = MissionType.WMS,
                        Priority = wmsPriority,
                        Status = MissionStatus.New,
                        TargetBay = bayNumber,
                        WmsId = wmsId,
                    });

                this.dataContext.SaveChanges();

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
                        FsmId = Guid.NewGuid(),
                        CreationDate = DateTime.Now,
                        LoadingUnitId = loadingUnitId,
                        TargetBay = bayNumber,
                        Status = MissionStatus.New,
                        MissionType = MissionType.IN
                    });

                this.dataContext.SaveChanges();

                this.logger.LogInformation("Created internal MAS recall mission.");

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
                    .Where(m => m.Status != MissionStatus.Completed && m.Status != MissionStatus.Aborted)
                    .OrderBy(o => o.Priority)
                    .ThenBy(o => o.CreationDate)
                    .ToArray();
            }
        }

        public IEnumerable<Mission> GetAllActiveMissionsByBay(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions
                    .Where(m => m.TargetBay == bayNumber)
                    .Where(m => m.Status != MissionStatus.Completed && m.Status != MissionStatus.Aborted)
                    .OrderBy(o => o.Priority)
                    .ThenBy(o => o.CreationDate)
                    .ToArray();
            }
        }

        public IEnumerable<Mission> GetAllExecutingMissions()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions
                    .Where(m => m.Status == MissionStatus.Executing || m.Status == MissionStatus.Waiting);
            }
        }

        public IEnumerable<Mission> GetAllWmsMissions()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions
                    .Where(m => m.WmsId != null)
                    .ToArray();
            }
        }

        public Mission GetByGuid(Guid fsmId)
        {
            lock (this.dataContext)
            {
                var mission = this.dataContext.Missions
                    .SingleOrDefault(m => m.FsmId == fsmId);
                if (mission is null)
                {
                    throw new EntityNotFoundException(nameof(mission));
                }
                return mission;
            }
        }

        public Mission GetById(int id)
        {
            lock (this.dataContext)
            {
                var mission = this.dataContext.Missions
                    .SingleOrDefault(m => m.Id == id);
                if (mission is null)
                {
                    throw new EntityNotFoundException(nameof(mission));
                }
                return mission;
            }
        }

        public bool IsMissionInWaitState(BayNumber bayNumber, int loadingUnitId)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions
                    .AsNoTracking()
                    .Any(m => m.TargetBay == bayNumber
                        && m.Status == MissionStatus.Waiting
                        && m.LoadingUnitId == loadingUnitId);
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
                        CommandAction.Stop);

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
                this.dataContext.Missions.Update(mission);

                this.dataContext.SaveChanges();
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
                var missions = this.dataContext.Missions
                    .AsNoTracking()
                    .Where(m => m.NeedHomingAxis == axis
                        && (bayNumber == BayNumber.ElevatorBay || m.TargetBay == bayNumber)
                        )
                    .ToArray();
                if (missions.Any())
                {
                    foreach (var mission in missions)
                    {
                        mission.NeedHomingAxis = Axis.None;
                        this.dataContext.Missions.Update(mission);

                        this.dataContext.SaveChanges();
                        this.logger.LogDebug($"Elevator Homing executed for Load Unit {mission.LoadingUnitId}");
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
