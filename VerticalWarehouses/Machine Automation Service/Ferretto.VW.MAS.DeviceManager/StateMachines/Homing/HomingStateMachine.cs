using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Homing.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Homing.Models;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Homing
{
    internal class HomingStateMachine : StateMachineBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly Calibration calibration;

        private readonly IHomingMachineData machineData;

        #endregion

        #region Constructors

        public HomingStateMachine(
            Axis axisToCalibrate,
            Calibration calibration,
            int? loadingUnitId,
            bool isOneTonMachine,
            bool showErrors,
            bool turnBack,
            BayNumber requestingBay,
            BayNumber targetBay,
            IMachineResourcesProvider machineResourcesProvider,
            IEventAggregator eventAggregator,
            ILogger logger,
            IBaysDataProvider baysDataProvider,
            IServiceScopeFactory serviceScopeFactory)
            : base(targetBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.calibration = calibration;

            this.machineData = new HomingMachineData(
                isOneTonMachine,
                loadingUnitId,
                requestingBay,
                targetBay,
                machineResourcesProvider,
                baysDataProvider.GetInverterIndexByAxis(axisToCalibrate, targetBay),
                showErrors,
                turnBack,
                eventAggregator,
                logger,
                serviceScopeFactory);
            this.machineData.RequestedAxisToCalibrate = axisToCalibrate;
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
                        MessageActor.DeviceManager,
                        MessageType.CalibrateAxis,
                        this.machineData.RequestingBay,
                        this.machineData.TargetBay,
                        MessageStatus.OperationExecuting);

                    this.PublishNotificationMessage(notificationMessage);
                }

                if (message.Status == MessageStatus.OperationEnd)
                {
                    if (this.machineData.AxisToCalibrate == Axis.Vertical)
                    {
                        using (var scope = this.ServiceScopeFactory.CreateScope())
                        {
                            var elevatorProvider = scope.ServiceProvider.GetRequiredService<IElevatorProvider>();
                            var distance = Math.Abs(elevatorProvider.VerticalPosition - this.machineData.VerticalStartingPosition);
                            if (distance > 100)
                            {
                                var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                                machineProvider.UpdateVerticalAxisStatistics(distance);
                            }
                        }
                    }

                    this.machineData.NumberOfExecutedSteps++;
                    this.machineData.InverterIndexOld = this.machineData.CurrentInverterIndex;
                    if (this.axisToCalibrate == Axis.HorizontalAndVertical)
                    {
                        if (this.machineData.NumberOfExecutedSteps == this.machineData.MaximumSteps - 1)
                        {
                            this.machineData.AxisToCalibrate =
                                (this.machineData.AxisToCalibrate == Axis.Vertical) ?
                                    Axis.Horizontal :
                                    Axis.Vertical;
                        }
                        else
                        {
                            this.machineData.CalibrationType = Calibration.ResetEncoder;
                        }
                    }
                    else if (this.calibration == Calibration.FindSensor)
                    {
                        this.machineData.CalibrationType = Calibration.ResetEncoder;
                    }

                    if (this.machineData.AxisToCalibrate == Axis.Vertical)
                    {
                        this.machineData.CalibrationType = Calibration.Elevator;
                        this.machineData.CurrentInverterIndex = InverterIndex.MainInverter;
                    }
                    else if (this.machineData.IsOneTonMachine && this.machineData.AxisToCalibrate == Axis.Horizontal)
                    {
                        this.machineData.CurrentInverterIndex = InverterIndex.Slave1;
                    }
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
                    this.machineData.CalibrationType = this.machineData.MachineSensorStatus.IsSensorZeroOnCradle ? Calibration.FindSensor : Calibration.ResetEncoder;
                    this.machineData.NumberOfExecutedSteps = 0;
                    this.machineData.MaximumSteps = this.machineData.MachineSensorStatus.IsSensorZeroOnCradle ? 3 : 2;
                    break;

                case Axis.Horizontal:
                    this.machineData.AxisToCalibrate = this.axisToCalibrate;
                    this.machineData.CalibrationType = this.machineData.MachineSensorStatus.IsSensorZeroOnCradle ? this.calibration : Calibration.ResetEncoder;
                    this.machineData.NumberOfExecutedSteps = 0;
                    this.machineData.MaximumSteps = (this.machineData.CalibrationType == Calibration.FindSensor) ? 2 : 1;
                    break;

                case Axis.BayChain:
                    this.machineData.AxisToCalibrate = this.axisToCalibrate;
                    this.machineData.CalibrationType = Calibration.FindSensor;
                    this.machineData.NumberOfExecutedSteps = 0;
                    this.machineData.MaximumSteps = 1;
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
                checkConditions = this.CheckConditions(out var errorText);
                if (!checkConditions)
                {
                    var notificationMessage = new NotificationMessage(
                        null,
                        errorText,
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.InverterException,
                        this.machineData.RequestingBay,
                        this.machineData.TargetBay,
                        MessageStatus.OperationStart);

                    this.PublishNotificationMessage(notificationMessage);

                    if (this.machineData.ShowErrors)
                    {
                        using (var scope = this.ServiceScopeFactory.CreateScope())
                        {
                            var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                            errorsProvider.RecordNew(DataModels.MachineErrorCode.ConditionsNotMetForHoming, this.machineData.RequestingBay);
                        }
                    }

                    this.Logger.LogError($"Conditions not verified for homing: {errorText}");

                    this.ChangeState(new HomingErrorState(stateData, this.Logger));
                }
                else
                {
                    this.ChangeState(new HomingStartState(stateData, this.Logger));
                }
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        private bool CheckConditions(out string errorText)
        {
            //HACK The condition must be handled by the Bug #3711
            //INFO For the Belt Burnishing the positioning is allowed only without a drawer.
            var ok = true;
            errorText = string.Empty;
            if (this.machineData.TargetBay == BayNumber.ElevatorBay)
            {
                if (this.machineData.MaximumSteps > 1
                    && !(this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle && !this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                    && !(this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle && this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                    )
                {
                    ok = false;
                    errorText = $"Invalid presence sensors: zero {this.machineData.MachineSensorStatus.IsSensorZeroOnCradle}, completely on board {this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle}";
                }
                else if (this.machineData.CalibrationType == Calibration.FindSensor
                    && !this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle
                    )
                {
                    ok = false;
                    errorText = "Find Zero not possible with full elevator";
                }
                else if (this.machineData.RequestedAxisToCalibrate == Axis.Vertical
                    || this.machineData.RequestedAxisToCalibrate == Axis.HorizontalAndVertical
                    )
                {
                    using (var scope = this.ServiceScopeFactory.CreateScope())
                    {
                        var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                        var bays = baysDataProvider.GetAll();
                        foreach (var bay in bays)
                        {
                            if (bay.Shutter != null
                                && bay.Shutter.Type != ShutterType.NotSpecified
                                )
                            {
                                var shutterInverter = bay.Shutter.Inverter.Index;
                                var shutterPosition = this.machineData.MachineSensorStatus.GetShutterPosition(shutterInverter);
                                if (shutterPosition == ShutterPosition.Intermediate || shutterPosition == ShutterPosition.Opened)
                                {
                                    ok = false;
                                    errorText = "Homing not possible with open shutter";
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    // Retrieve the bay
                    var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                    var bay = baysDataProvider.GetByNumber(this.machineData.TargetBay);

                    // Check if zero sensor is ON
                    if (this.machineData.CalibrationType == Calibration.FindSensor
                        && !this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.TargetBay)
                        && !bay.IsExternal)
                    {
                        ok = false;
                        errorText = "Find Zero not possible: Invalid Bay Zero sensor";
                    }

#if CHECK_BAY_SENSOR

                    // Handle carousel
                    if (bay.Carousel != null && !bay.IsExternal)
                    {
                        // Check presence in top position
                        //if (ok
                        //    && this.machineData.CalibrationType == Calibration.FindSensor
                        //    && this.machineData.MachineSensorStatus.IsDrawerInBayTop(this.machineData.TargetBay)
                        //    )
                        //{
                        //    ok = false;
                        //    errorText = "Find Zero not possible: Top position occupied";
                        //}

                        // Check presence in bottom position
                        if (ok
                            && this.machineData.CalibrationType == Calibration.FindSensor
                            && bay.Positions.Any(p => !p.IsBlocked && !p.IsUpper)
                            && this.machineData.MachineSensorStatus.IsDrawerInBayBottom(this.machineData.TargetBay)
                            )
                        {
                            ok = false;
                            errorText = "Find Zero not possible: Bottom position occupied";
                        }
                    }

                    // Handle external bay
                    if (bay.IsExternal)
                    {
                        // Check presence in external position
                        //if (ok
                        //    && this.machineData.CalibrationType == Calibration.FindSensor
                        //    && this.machineData.MachineSensorStatus.IsDrawerInBayTop(this.machineData.TargetBay)
                        //    )
                        //{
                        //    ok = false;
                        //    errorText = "Find Zero not possible: External position occupied";
                        //}

                        // Check presence in internal position
                        //if (ok
                        //    && this.machineData.CalibrationType == Calibration.FindSensor
                        //    && this.machineData.MachineSensorStatus.IsDrawerInBayBottom(this.machineData.TargetBay)
                        //    )
                        //{
                        //    ok = false;
                        //    errorText = "Find Zero not possible: Internal position occupied";
                        //}
                    }
                }
#endif
            }
            return ok;
        }

        #endregion
    }
}
