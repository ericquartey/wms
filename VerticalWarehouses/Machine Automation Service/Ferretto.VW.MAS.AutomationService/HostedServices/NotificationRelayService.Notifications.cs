﻿using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationRelayService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            Contract.Requires(notification != null);

            return
                notification.Destination is MessageActor.WebApi
                ||
                notification.Destination is MessageActor.AutomationService
                ||
                notification.Destination is MessageActor.Any;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            Contract.Requires(message != null);

            if (message.ErrorLevel is ErrorLevel.Fatal)
            {
                this.Logger.LogCritical(message.Description);
                this.applicationLifetime.StopApplication();
            }

            switch (message.Type)
            {
                case MessageType.SensorsChanged:
                    await this.OnSensorsChanged(message);
                    break;

                case MessageType.MachineMode:
                    await this.OnMachineModeChanged(message);
                    break;

                case MessageType.Homing:
                    await this.HomingMethod(message, serviceProvider);
                    break;

                case MessageType.SwitchAxis:
                    await this.SwitchAxisMethod(message);
                    break;

                case MessageType.ShutterPositioning:
                    await this.ShutterPositioningMethod(message);
                    break;

                case MessageType.CalibrateAxis:
                    await this.CalibrateAxisMethod(message);
                    break;

                case MessageType.ElevatorPosition when message.Data is ElevatorPositionMessageData:
                    await this.OnElevatorPositionChanged(message.Data as ElevatorPositionMessageData);
                    break;

                case MessageType.BayChainPosition when message.Data is BayChainPositionMessageData:
                    await this.OnBayChainPositionChanged(message.Data as BayChainPositionMessageData);
                    break;

                case MessageType.Positioning:
                    await this.OnPositioningChanged(message);
                    break;

                case MessageType.ResolutionCalibration:
                    await this.ResolutionCalibrationMethod(message);
                    break;

                case MessageType.AssignedMissionOperationChanged when message.Data is AssignedMissionOperationChangedMessageData:
                    await this.OnAssignedMissionOperationChanged(message.Data as AssignedMissionOperationChangedMessageData);
                    break;

                case MessageType.ElevatorWeightCheck:
                    await this.ElevatorWeightCheckMethod(message);
                    break;

                case MessageType.BayOperationalStatusChanged when message.Data is BayOperationalStatusChangedMessageData:
                    await this.OnBayConnected(message.Data as BayOperationalStatusChangedMessageData);
                    break;

                case MessageType.ErrorStatusChanged when message.Data is IErrorStatusMessageData:
                    await this.OnErrorStatusChanged(message.Data as IErrorStatusMessageData);
                    break;

                case MessageType.InverterStatusWord:
                    await this.OnInverterStatusWordChanged(message);
                    break;

                case MessageType.MachineStateActive:
                    await this.MachineStateActiveMethod(message);
                    break;

                case MessageType.MachineStatusActive:
                    await this.MachineStatusActiveMethod(message);
                    break;

                case MessageType.DataLayerReady:
                    this.OnDataLayerReady(serviceProvider);
                    break;

                case MessageType.ChangeRunningState:
                    await this.OnChangeRunningState(message);
                    break;

                case MessageType.MoveLoadingUnit:
                    await this.OnMoveLoadingUnit(message);
                    break;

                case MessageType.FsmException:
                    await this.OnFsmException(message);
                    break;

                case MessageType.BayLight:
                    await this.OnBayLight(message);
                    break;

                case MessageType.ProfileCalibration:
                    await this.OnProfileCalibration(message);
                    break;
            }
        }

        #endregion
    }
}
