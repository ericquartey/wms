using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MachineManager
{
    internal partial class MachineManagerService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return
                notification.Destination == MessageActor.MachineManager
                ||
                notification.Destination == MessageActor.Any;
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            switch (message.Type)
            {
                case MessageType.FaultStateChanged:
                case MessageType.RunningStateChanged:
                    this.OnMachineRunningStatusChange(message);
                    break;
            }

            return Task.CompletedTask;
        }

        private void OnMachineRunningStatusChange(NotificationMessage message)
        {
            if (message is null)
            {
                return;
            }

            if (message.Data is IStateChangedMessageData messageData)
            {
                var reason = StopRequestReason.NoReason;

                if (message.Type == MessageType.FaultStateChanged && messageData.CurrentState)
                {
                    reason = StopRequestReason.FaultStateChanged;
                }
                else if (message.Type == MessageType.RunningStateChanged && !messageData.CurrentState)
                {
                    reason = StopRequestReason.RunningStateChanged;
                }

                if (reason != StopRequestReason.NoReason)
                {
                    var newMessageData = new ChangeRunningStateMessageData(false, null, CommandAction.Start, reason);
                    var command = new CommandMessage(
                        newMessageData,
                        message.Description,
                        message.Destination,
                        message.Source,
                        MessageType.ChangeRunningState,
                        message.RequestingBay);

                    if (this.machineMissionsProvider.TryCreateMachineMission(FSMType.ChangeRunningType, command, out var missionId))
                    {
                        var errorCode = reason == StopRequestReason.FaultStateChanged
                            ? DataModels.MachineErrorCode.InverterFaultStateDetected
                            : DataModels.MachineErrorCode.SecurityWasTriggered;

                        this.serviceScope.ServiceProvider
                            .GetRequiredService<IErrorsProvider>()
                            .RecordNew(errorCode);

                        this.machineMissionsProvider.StartMachineMission(missionId, command);
                    }
                    else
                    {
                        this.Logger.LogError("Failed to create Change Running State machine mission");
                        this.NotifyCommandError(command);
                    }
                }
            }
        }

        #endregion
    }
}
