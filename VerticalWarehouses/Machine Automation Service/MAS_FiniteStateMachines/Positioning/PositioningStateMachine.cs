using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.SensorsStatus;
using Ferretto.VW.MAS_Utils.Messages;
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

        private readonly ILogger logger;

        private readonly MachineSensorsStatus machineSensorsStatus;

        private readonly IPositioningMessageData positioningMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningStateMachine(
            IEventAggregator eventAggregator,
            IPositioningMessageData positioningMessageData,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.logger = logger;

            this.logger.LogTrace("1:Method Start");

            this.logger.LogTrace($"TargetPosition = {positioningMessageData.TargetPosition} - CurrentPosition = {positioningMessageData.CurrentPosition} - MovementType = {positioningMessageData.MovementType}");

            this.CurrentState = new EmptyState(logger);

            this.positioningMessageData = positioningMessageData;

            this.machineSensorsStatus = machineSensorsStatus;
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
            this.logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
        }

        public override void Start()
        {
            bool checkVerticalConditions;
            bool checkHorizontalConditions;

            //INFO Begin check the pre conditions to start the positioning
            lock (this.CurrentState)
            {
                //INFO Check the Horizontal and Vertical conditions for Positioning
                checkVerticalConditions = this.CheckVerticalConditions();
                checkHorizontalConditions = this.CheckHorizontalConditions();

                if (checkVerticalConditions || checkHorizontalConditions)
                {
                    this.CurrentState = new PositioningErrorState(this, this.positioningMessageData, null, this.Logger);
                }
                else
                {
                    this.CurrentState = new PositioningStartState(this, this.positioningMessageData, this.logger);
                }

                this.CurrentState?.Start();
            }
            //INFO End check the pre conditions to start the positioning

            this.logger.LogTrace($"1:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop()
        {
            this.logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop();
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

        private bool CheckHorizontalConditions()
        {
            bool checkHorizontalConditions;

            checkHorizontalConditions = this.machineSensorsStatus.DrawerIsCompletelyOnCradle || this.machineSensorsStatus.DrawerIsPartiallyOnCradle ||
                                        //TEMP 7, 11  e 15
                                        (this.machineSensorsStatus.RawRemoteIOsInputs[(int)IOMachineSensors.LUPresentInBay1] ||
                                         this.machineSensorsStatus.RawRemoteIOsInputs[(int)IOMachineSensors.LUPresentInBay2] ||
                                         this.machineSensorsStatus.RawRemoteIOsInputs[(int)IOMachineSensors.LUPresentInBay3]) && this.positioningMessageData.AxisMovement == Axis.Horizontal;

            return checkHorizontalConditions;
        }

        private bool CheckVerticalConditions()
        {
            bool checkVerticalConditions;

            checkVerticalConditions = this.machineSensorsStatus.DrawerIsCompletelyOnCradle || this.machineSensorsStatus.DrawerIsPartiallyOnCradle ||
                                      !this.machineSensorsStatus.SensorInZeroOnCradle && this.positioningMessageData.AxisMovement == Axis.Vertical;

            return checkVerticalConditions;
        }

        #endregion
    }
}
