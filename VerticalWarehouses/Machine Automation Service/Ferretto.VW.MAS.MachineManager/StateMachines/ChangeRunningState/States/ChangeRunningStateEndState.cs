using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States
{
    internal class ChangeRunningStateEndState : StateBase, IChangeRunningStateEndState, IEndState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IMachineControlProvider machineControlProvider;

        private readonly IMachineVolatileDataProvider machineModeDataProvider;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();

        #endregion

        #region Constructors

        public ChangeRunningStateEndState(
            IMachineResourcesProvider machineResourcesProvider,
            IBaysDataProvider baysDataProvider,
            IMachineControlProvider machineControlProvider,
            IErrorsProvider errorsProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            ISensorsProvider sensorsProvider,
            IMissionsDataProvider missionsDataProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.machineControlProvider = machineControlProvider ?? throw new ArgumentNullException(nameof(machineControlProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.machineModeDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
        }

        #endregion

        #region Properties

        public NotificationMessage EndMessage { get; set; }

        public NotificationMessage ErrorMessage { get; set; }

        public bool IsCompleted { get; set; }

        public StopRequestReason StopRequestReason { get; set; }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"ChangeRunningStateEndState: received command {commandMessage.Type}, {commandMessage.Description}, reason {this.StopRequestReason}");
            this.EndMessage = new NotificationMessage(
                commandMessage.Data,
                commandMessage.Description,
                MessageActor.AutomationService,
                MessageActor.MachineManager,
                commandMessage.Type,
                commandMessage.RequestingBay,
                commandMessage.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.StopRequestReason));

            if (this.EndMessage.Data is IChangeRunningStateMessageData runningState)
            {
                if (this.StopRequestReason is StopRequestReason.NoReason
                    && this.sensorsProvider.IsMachineSecurityRunning == runningState.Enable)
                {
                    this.IsCompleted = true;

                    //this.machineModeDataProvider.Mode = MachineMode.Manual;
                    if (this.machineModeDataProvider.Mode != MachineMode.Shutdown)
                    {
                        this.machineModeDataProvider.Mode = this.machineModeDataProvider.GetMachineModeManualByBayNumber(commandMessage.TargetBay);
                        this.Logger.LogInformation($"Machine status switched to {this.machineModeDataProvider.Mode}; Running state {runningState.Enable}");
                    }

                    if (this.missionsDataProvider.GetAllExecutingMissions().Any(m => m.TargetBay == commandMessage.RequestingBay && m.Status == MissionStatus.Waiting))
                    {
                        this.baysDataProvider.Light(commandMessage.RequestingBay, true);
                        this.baysDataProvider.CheckIntrusion(commandMessage.RequestingBay, true);
                    }
                }
                else
                {
                    var result = this.AntiIntrusionBarrierDetect(commandMessage.RequestingBay);

                    this.Logger.LogWarning($"ChangeRunningStateEndState: Running state {runningState.Enable} not valid or error detected");

                    var endMessageData = new ChangeRunningStateMessageData(false, null, runningState.CommandAction, StopRequestReason.Error);
                    this.EndMessage = new NotificationMessage(
                        endMessageData,
                        commandMessage.Description,
                        MessageActor.AutomationService,
                        MessageActor.MachineManager,
                        commandMessage.Type,
                        commandMessage.RequestingBay,
                        commandMessage.TargetBay,
                        StopRequestReasonConverter.GetMessageStatusFromReason(StopRequestReason.Error));

                    if (!result && this.machineModeDataProvider.Mode != MachineMode.Shutdown)
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.ConditionsNotMetForRunning, commandMessage.RequestingBay);
                    }

                    {
                        var newMessageData = new ChangeRunningStateMessageData(false);

                        this.machineControlProvider.StartChangePowerStatus(newMessageData, MessageActor.MachineManager, commandMessage.RequestingBay);
                    }

                    {
                        var newMessageData = new StopMessageData(this.StopRequestReason);
                        this.machineControlProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, commandMessage.RequestingBay);
                    }
                }
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.machineControlProvider.PowerStatusChangeStatus(notification);

            if (notificationStatus != MessageStatus.NotSpecified)
            {
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationRunningStop:
                    case MessageStatus.OperationFaultStop:
                        this.UpdateResponseList(notificationStatus, notification.TargetBay);
                        break;
                }
            }
            else
            {
                notificationStatus = this.machineControlProvider.StopOperationStatus(notification);

                if (notificationStatus != MessageStatus.NotSpecified)
                {
                    switch (notificationStatus)
                    {
                        // State machine is in error, any response from device manager state machines will do to complete state machine shutdown
                        case MessageStatus.OperationError:
                        case MessageStatus.OperationEnd:
                            this.UpdateResponseList(notificationStatus, notification.TargetBay);
                            break;
                    }
                }
            }
            if (this.stateMachineResponses.Values.Count == this.baysDataProvider.GetBayNumbers().Count())
            {
                this.IsCompleted = true;
                this.Logger.LogInformation($"ChangeRunningStateEndState: Completed; Running state {this.sensorsProvider.IsMachineSecurityRunning}");
            }

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            return this;
        }

        private bool AntiIntrusionBarrierDetect(BayNumber bay)
        {
            this.Logger.LogWarning($"Check {bay} anti intrusion barrier");
            var errorCode = MachineErrorCode.NoError;
            switch (bay)
            {
                case BayNumber.BayOne:
                    if (this.machineResourcesProvider.IsAntiIntrusionBarrierBay1
                        || this.machineResourcesProvider.IsAntiIntrusionBarrier2Bay1)
                    {
                        errorCode = MachineErrorCode.SecurityBarrierWasTriggered;
                    }
                    break;

                case BayNumber.BayTwo:
                    if (this.machineResourcesProvider.IsAntiIntrusionBarrierBay2
                        || this.machineResourcesProvider.IsAntiIntrusionBarrier2Bay2)
                    {
                        errorCode = MachineErrorCode.SecurityBarrierWasTriggered;
                    }
                    break;

                case BayNumber.BayThree:
                    if (this.machineResourcesProvider.IsAntiIntrusionBarrierBay3
                        || this.machineResourcesProvider.IsAntiIntrusionBarrier2Bay3)
                    {
                        errorCode = MachineErrorCode.SecurityBarrierWasTriggered;
                    }
                    break;

                default:
                    break;
            }
            if (errorCode != MachineErrorCode.NoError)
            {
                this.errorsProvider.RecordNew(errorCode, bay);
                return true;
            }
            return false;
        }

        private void UpdateResponseList(MessageStatus status, BayNumber targetBay)
        {
            if (this.stateMachineResponses.TryGetValue(targetBay, out var stateMachineResponse))
            {
                stateMachineResponse = status;
                this.stateMachineResponses[targetBay] = stateMachineResponse;
            }
            else
            {
                this.stateMachineResponses.Add(targetBay, status);
            }
        }

        #endregion
    }
}
