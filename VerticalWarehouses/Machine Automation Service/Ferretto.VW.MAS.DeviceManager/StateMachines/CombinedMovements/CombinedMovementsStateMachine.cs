using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.CombinedMovements.Interfaces;
using Ferretto.VW.MAS.DeviceManager.CombinedMovements.Models;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.CombinedMovements
{
    internal class CombinedMovementsStateMachine : StateMachineBase
    {
        #region Fields

        private readonly ICombinedMovementsMachineData machineData;

        #endregion

        #region Constructors

        public CombinedMovementsStateMachine(
            MessageActor requester,
            BayNumber requestingBay,
            BayNumber targetBay,
            ICombinedMovementsMessageData messageData,
            IMachineResourcesProvider machineResourcesProvider,
            IEventAggregator eventAggregator,
            ILogger logger,
            IBaysDataProvider baysDataProvider,
            IServiceScopeFactory serviceScopeFactory)
            : base(targetBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.Logger.LogTrace("1:Method Start");

            this.Logger.LogTrace($"Combined movements on {targetBay}, requested by Bay: {requestingBay}");

            this.machineData = new CombinedMovementsMachineData(
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
            this.Logger.LogDebug($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}"); // LogTrace

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
                var stateData = new CombinedMovementsStateData(this, this.machineData);

                if (this.CheckConditions(out var errorText, out var errorCode))
                {
                    this.ChangeState(new CombinedMovementsStartState(stateData, this.Logger));
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
                    this.ChangeState(new CombinedMovementsErrorState(stateData, this.Logger));
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
        /// Check the conditions to apply the combined movements
        /// </summary>
        /// <param name="errorText"></param>
        /// <param name="errorCode"></param>
        /// <returns><c>true</c> if conditions are met, <c>false</c> otherwise</returns>
        private bool CheckConditions(out string errorText, out DataModels.MachineErrorCode errorCode)
        {
            errorText = string.Empty;
            errorCode = DataModels.MachineErrorCode.NoError;

            // Check if machine is 1T
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var machineVolatileDataProvider = scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                if (!machineVolatileDataProvider.IsOneTonMachine.Value)
                {
                    errorText = ErrorDescriptions.ConditionsNotMetForPositioning;
                    errorCode = DataModels.MachineErrorCode.ConditionsNotMetForPositioning;
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
