using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.HorizontalResolution.Interfaces;
using Ferretto.VW.MAS.DeviceManager.HorizontalResolution.Models;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.HorizontalResolution
{
    internal class HorizontalResolutionStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IHorizontalResolutionMachineData machineData;

        #endregion

        #region Constructors

        public HorizontalResolutionStateMachine(
            MessageActor requester,
            BayNumber requestingBay,
            BayNumber targetBay,
            IPositioningMessageData messageData,
            IMachineResourcesProvider machineResourcesProvider,
            IEventAggregator eventAggregator,
            ILogger logger,
            IBaysDataProvider baysDataProvider,
            IServiceScopeFactory serviceScopeFactory)
            : base(targetBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.Logger.LogTrace("1:Method Start");
            this.baysDataProvider = baysDataProvider;

            this.machineData = new HorizontalResolutionMachineData(
                requester,
                requestingBay,
                targetBay,
                messageData,
                machineResourcesProvider,
                baysDataProvider.GetInverterIndexByMovementType(messageData, targetBay),
                eventAggregator,
                logger,
                baysDataProvider,
                serviceScopeFactory);
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
                var stateData = new HorizontalResolutionStateData(this, this.machineData);
                //INFO Check the Horizontal and Vertical conditions for Positioning
                if (this.machineData.MessageData.BypassConditions ||
                    this.CheckConditions(out var errorText, out var errorCode))
                {
                    this.ChangeState(new HorizontalResolutionStartState(stateData, this.Logger));
                }
                else
                {
                    using (var scope = this.ServiceScopeFactory.CreateScope())
                    {
                        var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                        errorsProvider.RecordNew(errorCode, this.machineData.RequestingBay);
                    }

                    var notificationMessage = new NotificationMessage(
                        this.machineData.MessageData,
                        errorText,
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.InverterException,
                        this.machineData.RequestingBay,
                        this.machineData.TargetBay,
                        MessageStatus.OperationStart);

                    this.PublishNotificationMessage(notificationMessage);
                    this.ChangeState(new HorizontalResolutionErrorState(stateData, this.Logger));
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

        /// <summary>
        /// Check conditions to execute the state machine.
        /// </summary>
        /// <param name="errorText">Error text description</param>
        /// <param name="errorCode">Error code</param>
        /// <returns><c>true</c> if success, <c>false</c> otherwise</returns>
        private bool CheckConditions(out string errorText, out DataModels.MachineErrorCode errorCode)
        {
            var bay = this.baysDataProvider.GetByNumber(this.machineData.TargetBay);
            var ok = true;
            errorText = string.Empty;
            errorCode = DataModels.MachineErrorCode.ConditionsNotMetForPositioning;

            if (this.machineData.MessageData.MovementMode == MovementMode.HorizontalResolution)
            {
                ok = !(bay.Carousel != null || bay.IsExternal)
                    || this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.TargetBay);
                if (!ok)
                {
                    errorText = $"{ErrorDescriptions.SensorZeroBayNotActiveAtStart} in Bay {(int)this.machineData.TargetBay}";
                    errorCode = DataModels.MachineErrorCode.SensorZeroBayNotActiveAtStart;
                }
                else if (!this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                {
                    ok = false;
                    errorText = ErrorDescriptions.MissingZeroSensorWithEmptyElevator;
                    errorCode = DataModels.MachineErrorCode.MissingZeroSensorWithEmptyElevator;
                }
            }
            return ok;
        }

        #endregion
    }
}
