using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Statistics.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.Statistics
{
    internal class StatisticsStartState : StateBase
    {
        #region Fields

        private readonly Dictionary<InverterIndex, MessageStatus> inverterResponses = new Dictionary<InverterIndex, MessageStatus>();

        private readonly IStatisticsMachineData machineData;

        private readonly IStatisticsStateData stateData;

        #endregion

        #region Constructors

        public StatisticsStartState(IStatisticsStateData stateData, ILogger logger)
            : base(stateData?.ParentMachine, logger)
        {
            this.stateData = stateData ?? throw new ArgumentNullException(nameof(stateData));
            this.machineData = stateData.MachineData as IStatisticsMachineData ?? throw new ArgumentNullException(nameof(stateData));
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            if (message.Type != FieldMessageType.InverterStatistics)
            {
                return;
            }

            Enum.TryParse(typeof(InverterIndex), message.DeviceIndex.ToString(), out var messageInverterIndex);

            if (message.Status != MessageStatus.OperationStart &&
                message.Status != MessageStatus.OperationExecuting)
            {
                if (this.inverterResponses.TryGetValue((InverterIndex)messageInverterIndex, out var inverterResponse))
                {
                    inverterResponse = message.Status;
                    this.inverterResponses[(InverterIndex)messageInverterIndex] = inverterResponse;
                }
                else
                {
                    this.inverterResponses.Add((InverterIndex)messageInverterIndex, message.Status);
                }
            }

            if (this.inverterResponses.Values.Count > 0)
            {
                if (this.inverterResponses.Values.Any(r => r != MessageStatus.OperationEnd))
                {
                    this.stateData.FieldMessage = message;
                    this.ParentStateMachine.ChangeState(new StatisticsErrorState(this.stateData, this.Logger));
                }
                else
                {
                    this.ParentStateMachine.ChangeState(new StatisticsEndState(this.stateData, this.Logger));
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name}");

            var commandMessage = new FieldCommandMessage(
                null,
                $"Get Statistics from Inverter",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterStatistics,
                (byte)this.machineData.InverterIndex);

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                        null,
                        $"Starting get statistics from inverter",
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.InverterStatistics,
                        this.machineData.RequestingBay,
                        this.machineData.TargetBay,
                        MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");
            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new StatisticsEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}
