using System;
using System.Linq;
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
    public class MissionMoveBayChainState : MissionMoveBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MissionMoveBayChainState(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
            this.baysDataProvider = this.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.errorsProvider = this.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.missionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
            this.loadingUnitMovementProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitMovementProvider>();

            this.logger = this.ServiceProvider.GetRequiredService<ILogger<MachineManagerService>>();
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
        }

        public override bool OnEnter(CommandMessage command)
        {
            if (command != null
                && command.Data is IMoveLoadingUnitMessageData messageData
                )
            {
                this.Mission.FsmStateName = nameof(MissionMoveBayChainState);
                this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                this.Mission.CloseShutterBayNumber = BayNumber.None;
                this.missionsDataProvider.Update(this.Mission);
                this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

                var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                if (bay is null)
                {
                    var description = $"{this.GetType().Name}: destination bay not found {this.Mission.LoadingUnitDestination}";

                    throw new StateMachineException(description, command, MessageActor.MachineManager);
                }
                if (this.loadingUnitMovementProvider.IsOnlyBottomPositionOccupied(bay.Number))
                {
                    this.Mission.RestoreConditions = false;
                }
                if (!this.loadingUnitMovementProvider.MoveCarousel(this.Mission.LoadingUnitId, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions))
                {
                    var description = $"MoveLoadingUnitBayChainState: Move Bay chain not possible in bay {bay.Number}. Wait for resume";
                    // we don't want an exception here because this is the normal procedure:
                    // send a second LU in lower position while operator is working on upper position
                    this.logger.LogDebug(description);
                }

                this.Mission.Status = MissionStatus.Waiting;

                this.Mission.RestoreConditions = false;
                this.missionsDataProvider.Update(this.Mission);
            }
            else
            {
                var description = $"Move Load Unit Bay Chain State received wrong initialization data ({command.Data.GetType().Name})";

                throw new StateMachineException(description, command, MessageActor.MachineManager);
            }

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.loadingUnitMovementProvider.CarouselStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.RequestingBay == this.Mission.TargetBay)
                    {
                        if (notification.Type == MessageType.Positioning)
                        {
                            var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                            var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
                            if (destination is null)
                            {
                                throw new StateMachineException($"Upper position not defined for bay {bay.Number}", null, MessageActor.MachineManager);
                            }
                            this.Mission.LoadingUnitDestination = destination.Location;

                            var newMessageData = new MoveLoadingUnitMessageData(
                                this.Mission.MissionType,
                                this.Mission.LoadingUnitSource,
                                this.Mission.LoadingUnitDestination,
                                this.Mission.LoadingUnitCellSourceId,
                                this.Mission.DestinationCellId,
                                this.Mission.LoadingUnitId,
                                (this.Mission.LoadingUnitDestination == LoadingUnitLocation.Cell),
                                false,
                                this.Mission.FsmId,
                                this.Mission.Action);

                            var msg = new NotificationMessage(
                                newMessageData,
                                $"Loading Unit {this.Mission.LoadingUnitId} placed on bay {bay.Number}",
                                MessageActor.AutomationService,
                                MessageActor.MachineManager,
                                MessageType.MoveLoadingUnit,
                                notification.RequestingBay,
                                bay.Number,
                                MessageStatus.OperationWaitResume);
                            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                            if (this.Mission.RestoreConditions)
                            {
                                this.loadingUnitMovementProvider.UpdateLastBayChainPosition(this.Mission.TargetBay);
                                this.Mission.RestoreConditions = false;
                            }
                            if (this.Mission.NeedHomingAxis == Axis.BayChain)
                            {
                                this.logger.LogDebug($"Homing Bay occupied start");
                                this.loadingUnitMovementProvider.Homing(Axis.BayChain, Calibration.FindSensor, this.Mission.LoadingUnitId, notification.RequestingBay, MessageActor.MachineManager);
                            }
                            else
                            {
                                this.BayChainEnd();
                            }
                        }
                        else if (notification.Type == MessageType.Homing)
                        {
                            this.BayChainEnd();
                        }
                    }
                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                    this.OnStop(StopRequestReason.Error);
                    break;
            }
        }

        private void BayChainEnd()
        {
            if (this.Mission.WmsId.HasValue)
            {
                var newStep = new MissionMoveWaitPickState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        public override void OnResume(CommandMessage command)
        {
#if CHECK_BAY_SENSOR
            if (!this.sensorsProvider.IsLoadingUnitInLocation(this.Mission.LoadingUnitDestination))
#endif
            {
                if (command.Data is IMoveLoadingUnitMessageData messageData)
                {
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                    if (!this.loadingUnitMovementProvider.MoveCarousel(this.Mission.LoadingUnitId, MessageActor.MachineManager, bay.Number, false))
                    {
                        var description = $"MoveLoadingUnitBayChainState: Move Bay chain not possible in bay {bay.Number}. Wait for another resume";
                        throw new StateMachineException(description, command, MessageActor.MachineManager);
                    }
                }
            }
#if CHECK_BAY_SENSOR
            else
            {
                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitNotRemoved, this.Mission.TargetBay);
            }
#endif
        }

        #endregion
    }
}
