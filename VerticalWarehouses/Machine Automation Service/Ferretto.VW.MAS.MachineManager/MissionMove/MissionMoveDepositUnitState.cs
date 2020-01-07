using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels.Resources;
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
    public class MissionMoveDepositUnitState : MissionMoveBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public MissionMoveDepositUnitState(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
            this.missionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
            this.loadingUnitMovementProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitMovementProvider>();
            this.cellsProvider = this.ServiceProvider.GetRequiredService<ICellsProvider>();
            this.errorsProvider = this.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.baysDataProvider = this.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.sensorsProvider = this.ServiceProvider.GetRequiredService<ISensorsProvider>();
            this.elevatorDataProvider = this.ServiceProvider.GetRequiredService<IElevatorDataProvider>();

            this.logger = this.ServiceProvider.GetRequiredService<ILogger<MachineManagerService>>();
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
        }

        public override bool OnEnter(CommandMessage command)
        {
            this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
            this.Mission.FsmStateName = nameof(MissionMoveDepositUnitState);
            this.missionsDataProvider.Update(this.Mission);

            this.Mission.Direction = HorizontalMovementDirection.Backwards;
            var bayNumber = this.Mission.TargetBay;
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            switch (this.Mission.LoadingUnitDestination)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.DestinationCellId != null)
                    {
                        var cell = this.cellsProvider.GetById(this.Mission.DestinationCellId.Value);

                        this.Mission.Direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    }

                    break;

                default:
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                    if (bay is null)
                    {
                        var description = $"{this.GetType().Name}: destination bay not found {this.Mission.LoadingUnitDestination}";

                        throw new StateMachineException(description);
                    }
                    this.Mission.Direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    bayNumber = bay.Number;
                    this.Mission.OpenShutterPosition = this.loadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadingUnitDestination);
                    if (this.Mission.OpenShutterPosition == this.sensorsProvider.GetShutterPosition(bay.Number))
                    {
                        this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                    }
                    if (bay.Carousel != null)
                    {
                        var result = this.loadingUnitMovementProvider.CheckBaySensors(bay, this.Mission.LoadingUnitDestination, deposit: true);
                        if (result != MachineErrorCode.NoError)
                        {
                            var error = this.errorsProvider.RecordNew(result);
                            throw new StateMachineException(error.Description);
                        }
                    }
                    break;
            }

            if (this.Mission.NeedHomingAxis == Axis.Horizontal)
            {
                if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                {
                    this.logger.LogDebug($"Open Shutter");
                    this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, this.Mission.RestoreConditions);
                }
                else
                {
                    this.logger.LogDebug($"Manual Horizontal forward positioning start");
                    this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadingUnitId, MessageActor.MachineManager, this.Mission.TargetBay);
                }
            }
            else
            {
                this.logger.LogDebug($"MoveLoadingUnit start: direction {this.Mission.Direction}, openShutter {this.Mission.OpenShutterPosition}");
                this.loadingUnitMovementProvider.MoveLoadingUnit(this.Mission.Direction, false, this.Mission.OpenShutterPosition, false, MessageActor.MachineManager, bayNumber, null);
            }
            this.Mission.RestoreConditions = false;
            this.missionsDataProvider.Update(this.Mission);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
        }

        #endregion
    }
}
