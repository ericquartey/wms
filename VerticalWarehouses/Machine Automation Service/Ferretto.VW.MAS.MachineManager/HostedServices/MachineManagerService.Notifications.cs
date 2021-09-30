using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            var missionMoveProvider = serviceProvider.GetRequiredService<IMissionMoveProvider>();
            switch (message.Type)
            {
                case MessageType.FaultStateChanged:
                case MessageType.RunningStateChanged:
                    lock (this.syncMachineObject)
                    {
                        this.OnMachineRunningStatusChange(message, serviceProvider);
                    }
                    lock (this.syncObject)
                    {
                        missionMoveProvider.OnNotification(message, serviceProvider);
                    }
                    break;

                case MessageType.DataLayerReady:
                    {
                        var machineVolatileDataProvider = serviceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                        machineVolatileDataProvider.IsAutomationServiceReady = true;
                        this.Logger.LogInformation("Machine Automation Service ready");
                    }
                    break;

                case MessageType.Positioning:
                case MessageType.Stop:
                case MessageType.InverterStop:
                case MessageType.ShutterPositioning:
                case MessageType.Homing:
                case MessageType.CombinedMovements:
                case MessageType.CheckIntrusion:
                case MessageType.ErrorStatusChanged:
                    lock (this.syncObject)
                    {
                        missionMoveProvider.OnNotification(message, serviceProvider);
                    }
                    break;
            }

            return Task.CompletedTask;
        }

        private void OnMachineRunningStatusChange(NotificationMessage message, IServiceProvider serviceProvider)
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

                    var machineMissionsProvider = serviceProvider.GetRequiredService<IMachineMissionsProvider>();
                    if (machineMissionsProvider.TryCreateMachineMission(FsmType.ChangeRunningType, command, out var missionId))
                    {
                        machineMissionsProvider.StartMachineMission(missionId, command);
                    }
                    else
                    {
                        this.Logger.LogWarning("Failed to create Change Running State machine mission");
                        var activeMachineMission = machineMissionsProvider.GetMissionsByType(FsmType.ChangeRunningType, MissionType.Manual).FirstOrDefault();
                        if (activeMachineMission != null)
                        {
                            machineMissionsProvider.StopMachineMission(activeMachineMission.FsmId, StopRequestReason.Stop);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
