using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveWaitPickState : MissionMoveBase
    {
        #region Fields

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        #endregion

        #region Constructors

        public MissionMoveWaitPickState(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
            this.baysDataProvider = this.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.missionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
            this.loadingUnitsDataProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
            this.loadingUnitMovementProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitMovementProvider>();
            this.sensorsProvider = this.ServiceProvider.GetRequiredService<ISensorsProvider>();

            this.logger = this.ServiceProvider.GetRequiredService<ILogger<MachineManagerService>>();
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
        }

        public override bool OnEnter(CommandMessage command)
        {
            this.Mission.FsmRestoreStateName = null;
            this.Mission.FsmStateName = nameof(MissionMoveWaitPickState);
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.missionsDataProvider.Update(this.Mission);
            this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);

            this.loadingUnitMovementProvider.NotifyAssignedMissionOperationChanged(bay.Number, this.Mission.WmsId.Value);

            if (this.Mission.LoadingUnitId > 0)
            {
                this.loadingUnitsDataProvider.SetHeight(this.Mission.LoadingUnitId, 0);
            }
            this.Mission.Status = MissionStatus.Waiting;
            this.Mission.RestoreConditions = false;
            this.missionsDataProvider.Update(this.Mission);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
        }

        public override void OnResume(CommandMessage command)
        {
#if CHECK_BAY_SENSOR
            if (!this.sensorsProvider.IsLoadingUnitInLocation(this.ejectBay))
#endif
            {
                if (command != null
                    && command.Data is IMoveLoadingUnitMessageData messageData
                    )
                {
                    var ejectBayLocation = this.Mission.LoadingUnitDestination;
                    var bayPosition = this.baysDataProvider.GetPositionByLocation(ejectBayLocation);
                    if (messageData.MissionType == MissionType.NoType)
                    {
                        // Remove LoadUnit

                        var lu = bayPosition.LoadingUnit?.Id ?? throw new EntityNotFoundException($"LoadingUnit by BayPosition ID={bayPosition.Id}");

                        this.baysDataProvider.RemoveLoadingUnit(lu);

                        var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    else
                    {
                        // Update mission and start moving
                        this.Mission.MissionType = messageData.MissionType;
                        this.Mission.WmsId = messageData.WmsId;
                        this.Mission.LoadingUnitSource = bayPosition.Location;
                        if (messageData.Destination == LoadingUnitLocation.Cell)
                        {
                            // prepare for finding a new empty cell
                            this.Mission.DestinationCellId = null;
                            this.Mission.LoadingUnitDestination = LoadingUnitLocation.Cell;
                        }
                        else if (bayPosition.Location != messageData.Destination)
                        {
                            // bay to bay movement
                            this.Mission.LoadingUnitDestination = messageData.Destination;
                        }
                        this.missionsDataProvider.Update(this.Mission);
                        var newStep = new MissionMoveStartState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                }
                else
                {
                    throw new StateMachineException($"{this.GetType().Name}:OnResume: Invalid command");
                }
            }
#if CHECK_BAY_SENSOR
            else
            {
                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitNotRemoved, this.requestingBay);
            }
#endif
        }

        #endregion
    }
}
