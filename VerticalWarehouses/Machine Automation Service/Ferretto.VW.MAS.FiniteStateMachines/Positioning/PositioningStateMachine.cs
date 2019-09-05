using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.Positioning.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Positioning.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning
{
    public class PositioningStateMachine : StateMachineBase
    {

        #region Fields

        private readonly IPositioningMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningStateMachine(
            BayIndex requestingBay,
            BayIndex targetBay,
            IPositioningMessageData messageData,
            IMachineSensorsStatus machineSensorsStatus,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IBaysProvider baysProvider,
            IServiceScopeFactory serviceScopeFactory)
            : base(requestingBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.CurrentState = new EmptyState(this.Logger);

            this.Logger.LogTrace("1:Method Start");

            this.Logger.LogTrace($"TargetPosition = {messageData.TargetPosition} - CurrentPosition = {messageData.CurrentPosition} - MovementType = {messageData.MovementType}");

            this.machineData = new PositioningMachineData(
                requestingBay,
                targetBay,
                messageData,
                machineSensorsStatus,
                baysProvider.GetInverterIndexByMovementType(messageData, targetBay),
                eventAggregator,
                logger,
                baysProvider,
                serviceScopeFactory);
        }

        #endregion

        #region Destructors

        ~PositioningStateMachine()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
        }

        public override void Start()
        {
            //INFO Begin check the pre conditions to start the positioning
            lock (this.CurrentState)
            {
                //INFO Check the Horizontal and Vertical conditions for Positioning
                var checkConditions = this.CheckConditions();

                var stateData = new PositioningStateData(this, this.machineData);

                if (checkConditions)
                {
                    if (this.machineData.MessageData.MovementMode == MovementMode.FindZero && this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                    {
                        this.CurrentState = new PositioningEndState(stateData);
                    }
                    else
                    {
                        this.CurrentState = new PositioningStartState(stateData);
                    }
                }
                else
                {
                    this.CurrentState = new PositioningErrorState(stateData);
                }

                this.CurrentState?.Start();
            }
            //INFO End check the pre conditions to start the positioning

            this.Logger.LogTrace($"1:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        private bool CheckConditions()
        {
            //HACK The condition must be handled by the Bug #3711
            //INFO For the Belt Burnishing the positioning is allowed only without a drawer.
            var checkConditions = ((this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle && this.machineData.MessageData.MovementMode == MovementMode.Position) ||
                                    this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle /*&& this.machineSensorsStatus.IsSensorZeroOnCradle*/) &&
                                    this.machineData.MessageData.AxisMovement == Axis.Vertical ||
                                    this.machineData.MessageData.AxisMovement == Axis.Horizontal;

            return checkConditions;
        }

        #endregion
    }
}
