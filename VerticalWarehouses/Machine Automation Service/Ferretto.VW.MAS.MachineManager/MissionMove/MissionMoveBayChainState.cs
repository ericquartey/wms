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
            this.Mission.FsmRestoreStateName = null;
            this.Mission.FsmStateName = nameof(MissionMoveBayChainState);
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.missionsDataProvider.Update(this.Mission);
            this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
            if (bay is null)
            {
                var description = $"{this.GetType().Name}: destination bay not found {this.Mission.LoadingUnitDestination}";

                throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            if (this.loadingUnitMovementProvider.IsOnlyBottomPositionOccupied(bay.Number))
            {
                this.Mission.RestoreConditions = false;
            }
            try
            {
                this.loadingUnitMovementProvider.MoveCarousel(this.Mission.LoadingUnitId, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);
            }
            catch (StateMachineException ex)
            {
                var description = $"Move Bay chain not possible in bay {bay.Number}. Reason {ex.Message}. Wait for resume";
                // we don't want any exception here because this is the normal procedure:
                // send a second LU in lower position while operator is working on upper position
                this.logger.LogDebug(description);
            }
            this.Mission.Status = MissionStatus.Waiting;

            this.Mission.RestoreConditions = false;
            this.missionsDataProvider.Update(this.Mission);

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
                                var description = $"Upper position not defined for bay {bay.Number}";
                                throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            this.Mission.LoadingUnitDestination = destination.Location;

                            var notificationText = $"Load Unit {this.Mission.LoadingUnitId} placed on bay {bay.Number}";
                            this.SendMoveNotification(bay.Number, notificationText, false, MessageStatus.OperationWaitResume);

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
            this.missionsDataProvider.Update(this.Mission);
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
                var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                try
                {
                    this.loadingUnitMovementProvider.MoveCarousel(this.Mission.LoadingUnitId, MessageActor.MachineManager, bay.Number, false);
                }
                catch (StateMachineException ex)
                {
                    var description = $"Move Bay chain not possible in bay {bay.Number}; reason {ex.Message}. Wait for another resume";
                    throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
                }
            }
#if CHECK_BAY_SENSOR
            else
            {
                var error = this.errorsProvider.RecordNew(result);
                var description = $"Move Bay chain not possible in bay {bay.Number}; reason {error.Description}. Wait for another resume";
                throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
            }
#endif
        }

        #endregion
    }
}
