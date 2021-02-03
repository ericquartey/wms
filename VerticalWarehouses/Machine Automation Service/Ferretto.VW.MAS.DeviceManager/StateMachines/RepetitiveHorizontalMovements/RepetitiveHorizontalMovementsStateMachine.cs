using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements.Interfaces;
using Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements.Models;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Ferretto.VW.MAS.DataModels.Resources;

namespace Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements
{
    internal class RepetitiveHorizontalMovementsStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IRepetitiveHorizontalMovementsMachineData machineData;

        #endregion

        #region Constructors

        public RepetitiveHorizontalMovementsStateMachine(
            MessageActor requester,
            BayNumber requestingBay,
            BayNumber targetBay,
            IRepetitiveHorizontalMovementsMessageData messageData,
            IMachineResourcesProvider machineResourcesProvider,
            IEventAggregator eventAggregator,
            ILogger logger,
            IBaysDataProvider baysDataProvider,
            IServiceScopeFactory serviceScopeFactory)
            : base(targetBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.Logger.LogTrace("1:Method Start");

            this.Logger.LogTrace($"Repetitive movements on Bay: {targetBay}");

            this.machineData = new RepetitiveHorizontalMovementsMachineData(
                requester,
                requestingBay,
                targetBay,
                messageData,
                machineResourcesProvider,
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
            this.Logger.LogDebug($"1:Start {this.GetType().Name}");

            lock (this.CurrentState)
            {
                var stateData = new RepetitiveHorizontalMovementsStateData(this, this.machineData);

                if (this.CheckConditions(out var errorText, out var errorCode))
                {
                    this.ChangeState(new RepetitiveHorizontalMovementsStartState(stateData, this.Logger));
                }
                else
                {
                    // Add errors
                    using (var scope = this.ServiceScopeFactory.CreateScope())
                    {
                        var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
                        errorsProvider.RecordNew(errorCode, this.machineData.RequestingBay);
                    }

                    // Publish a notification message about the error
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

                    // Change state
                    this.ChangeState(new RepetitiveHorizontalMovementsErrorState(stateData, this.Logger));
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
        /// Check the conditions in order to apply correctly the transition to Start state.
        /// </summary>
        /// <param name="errorText">Error description</param>
        /// <param name="errorCode">Error code (@see MachineErrorCode)</param>
        /// <returns></returns>
        private bool CheckConditions(out string errorText, out DataModels.MachineErrorCode errorCode)
        {
            var ok = true;
            errorText = string.Empty;
            errorCode = DataModels.MachineErrorCode.NoError;

            // --------------------
            // Check the conditions
            //  Table:
            //      + ExternalBay         => NOT ALLOWED
            //      + CarouselBay         => Drawer must be upper location
            //      + Internal SingleBay  => Drawer must be upper location
            //      + Internal DoubleBay  => Drawer must be upper location

            // Get the bay (related to the requesting bay number)
            var currentBay = this.machineData.BaysDataProvider.GetByNumber(this.machineData.RequestingBay);
            // Get the position bay
            var bayPosition = this.machineData.BaysDataProvider.GetPositionById(this.machineData.MessageData.BayPositionId);

            // External bay
            if (currentBay.IsExternal)
            {
                // TODO: Add these codes in the DataModels.Errors
                errorText = ErrorDescriptions.InvalidBay;
                errorCode = DataModels.MachineErrorCode.InvalidBay;
                return false;
            }

            // Carousel bay
            if (currentBay.Carousel != null)
            {
                // Check if bay position is upper
                if (!bayPosition.IsUpper)
                {
                    // TODO: Add these codes in the DataModels.Errors
                    errorText = ErrorDescriptions.InvalidPositionBay;
                    errorCode = DataModels.MachineErrorCode.InvalidPositionBay;
                    return false;
                }
                // Check if drawer is located in the top location
                if (!this.machineData.MachineSensorStatus.IsDrawerInBayTop(this.machineData.RequestingBay))
                {
                    errorText = ErrorDescriptions.TopLevelBayEmpty;
                    errorCode = DataModels.MachineErrorCode.TopLevelBayEmpty;
                    return false;
                }
            }

            // Internal bay (single or double type)
            if (currentBay.IsDouble && !bayPosition.IsUpper)
            {
                // TODO: Add these codes in the DataModels.Errors
                errorText = ErrorDescriptions.InvalidPositionBay;
                errorCode = DataModels.MachineErrorCode.InvalidPositionBay;
                return false;
            }
            // Check if drawer is located in the top location
            if (!this.machineData.MachineSensorStatus.IsDrawerInBayTop(this.machineData.RequestingBay))
            {
                errorText = ErrorDescriptions.TopLevelBayEmpty;
                errorCode = DataModels.MachineErrorCode.TopLevelBayEmpty;
                return false;
            }

            // Check shutter positions for all bays
            var bays = this.machineData.BaysDataProvider.GetAll();
            foreach (var bay in bays)
            {
                if (bay.Shutter != null
                    && bay.Shutter.Type != ShutterType.NotSpecified
                    )
                {
                    if (bay.Id == this.machineData.BaysDataProvider.GetByNumber(this.machineData.RequestingBay)?.Id)
                    {
                        // check the target bay
                        var shutterInverter = bay.Shutter.Inverter.Index;
                        var shutterPosition = this.machineData.MachineSensorStatus.GetShutterPosition(shutterInverter);
                        if (shutterPosition == ShutterPosition.Intermediate || shutterPosition == ShutterPosition.Closed)
                        {
                            errorText = $"Repetitive horizontal movements not possible with closed/intermediate shutter at bay {bay.Id}";
                            errorCode = DataModels.MachineErrorCode.ConditionsNotMetForPositioning;
                            ok = false;
                            break;
                        }
                    }
                    else
                    {
                        // check other bays
                        var shutterInverter = bay.Shutter.Inverter.Index;
                        var shutterPosition = this.machineData.MachineSensorStatus.GetShutterPosition(shutterInverter);
                        if (shutterPosition == ShutterPosition.Intermediate || shutterPosition == ShutterPosition.Opened)
                        {
                            errorText = $"Repetitive horizontal movements not possible with open shutter at bay {bay.Id}";
                            errorCode = DataModels.MachineErrorCode.ConditionsNotMetForPositioning;
                            ok = false;
                            break;
                        }
                    }
                }
            }

            return ok;
        }

        #endregion
    }
}
