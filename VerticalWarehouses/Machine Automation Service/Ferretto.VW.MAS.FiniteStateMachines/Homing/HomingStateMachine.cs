using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Homing.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Homing.Models;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Homing
{
    internal class HomingStateMachine : StateMachineBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly IHomingMachineData machineData;

        #endregion

        #region Constructors

        public HomingStateMachine(
            Axis axisToCalibrate,
            bool isOneKMachine,
            BayNumber requestingBay,
            BayNumber targetBay,
            IMachineResourcesProvider machineResourcesProvider,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.CurrentState = new EmptyState(this.Logger);

            this.axisToCalibrate = axisToCalibrate;

            this.machineData = new HomingMachineData(isOneKMachine, requestingBay, targetBay, machineResourcesProvider, eventAggregator, logger, serviceScopeFactory);
        }

        #endregion

        #region Destructors

        ~HomingStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
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

            if (message.Type == FieldMessageType.CalibrateAxis)
            {
                if (message.Status == MessageStatus.OperationExecuting)
                {
                    var notificationMessageData = new CalibrateAxisMessageData(this.machineData.AxisToCalibrate, this.machineData.NumberOfExecutedSteps + 1, this.machineData.MaximumSteps, MessageVerbosity.Info);
                    var notificationMessage = new NotificationMessage(
                        notificationMessageData,
                        $"{this.machineData.AxisToCalibrate} axis calibration executing",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.CalibrateAxis,
                        this.machineData.RequestingBay,
                        this.machineData.TargetBay,
                        MessageStatus.OperationExecuting);

                    this.PublishNotificationMessage(notificationMessage);
                }

                if (message.Status == MessageStatus.OperationEnd)
                {
                    this.machineData.NumberOfExecutedSteps++;
                    if (this.machineData.MaximumSteps == 3)
                    {
                        this.machineData.AxisToCalibrate =
                            (this.machineData.AxisToCalibrate == Axis.Vertical) ?
                                Axis.Horizontal :
                                Axis.Vertical;
                    }
                    if (this.machineData.AxisToCalibrate == Axis.Horizontal && this.machineData.NumberOfExecutedSteps > 0)
                    {
                        this.machineData.CalibrationType = Calibration.ResetEncoder;
                    }
                }
            }

            if (message.Type == FieldMessageType.InverterStatusUpdate &&
                message.Status == MessageStatus.OperationExecuting)
            {
                if (message.Data is InverterStatusUpdateFieldMessageData data &&
                    data.CurrentPosition.HasValue)
                {
                    var notificationMessageData = new CurrentPositionMessageData(data.CurrentPosition.Value);
                    var notificationMessage = new NotificationMessage(
                        notificationMessageData,
                        $"Current Encoder position: {data.CurrentPosition}",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.CurrentPosition,
                        this.machineData.RequestingBay,
                        this.machineData.TargetBay,
                        MessageStatus.OperationExecuting);

                    this.PublishNotificationMessage(notificationMessage);
                }
            }

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void PublishNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Publish Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            base.PublishNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            bool checkConditions;

            this.Logger.LogTrace("1:Method Start");
            switch (this.axisToCalibrate)
            {
                case Axis.HorizontalAndVertical:
                    this.machineData.AxisToCalibrate = Axis.Horizontal;
                    this.machineData.CalibrationType = Calibration.FindSensor;
                    this.machineData.NumberOfExecutedSteps = 0;
                    this.machineData.MaximumSteps = 3;
                    break;

                case Axis.Horizontal:
                    this.machineData.AxisToCalibrate = Axis.Horizontal;
                    this.machineData.CalibrationType = Calibration.FindSensor;
                    this.machineData.NumberOfExecutedSteps = 0;
                    this.machineData.MaximumSteps = 2;
                    break;

                case Axis.Vertical:
                    this.machineData.AxisToCalibrate = Axis.Vertical;
                    this.machineData.CalibrationType = Calibration.FindSensor;
                    this.machineData.NumberOfExecutedSteps = 0;
                    this.machineData.MaximumSteps = 1;
                    break;
            }

            lock (this.CurrentState)
            {
                var stateData = new HomingStateData(this, this.machineData);

                //INFO Check the Horizontal and Vertical conditions for Positioning
                checkConditions = this.CheckConditions();
                if (!checkConditions)
                {
                    var notificationMessage = new NotificationMessage(
                        null,
                        "Conditions not verified for homing",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.InverterException,
                        this.machineData.RequestingBay,
                        this.machineData.TargetBay,
                        MessageStatus.OperationStart);

                    this.PublishNotificationMessage(notificationMessage);

                    using (var scope = this.ServiceScopeFactory.CreateScope())
                    {
                        var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                        errorsProvider.RecordNew(DataModels.MachineErrors.ConditionsNotMetForPositioning, this.machineData.RequestingBay);
                    }

                    this.Logger.LogError($"Conditions not verified for homing: Missing Zero sensor with empty elevator OR Zero sensor active with full elevator");

                    this.CurrentState = new HomingErrorState(stateData);
                }
                else
                {
                    this.CurrentState = new HomingStartState(stateData);
                }
                this.CurrentState.Start();
            }

            this.Logger.LogTrace($"2:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        private bool CheckConditions()
        {
            //HACK The condition must be handled by the Bug #3711
            //INFO For the Belt Burnishing the positioning is allowed only without a drawer.
            var checkConditions = ((this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle && !this.machineData.MachineSensorStatus.IsSensorZeroOnCradle) ||
                                    this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle && this.machineData.MachineSensorStatus.IsSensorZeroOnCradle);

            return checkConditions;
        }

        #endregion
    }
}
