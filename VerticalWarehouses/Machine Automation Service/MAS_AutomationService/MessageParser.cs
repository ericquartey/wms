﻿using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_AutomationService
{
    public static class MessageParser
    {
        #region Methods

        public static ActionUpdateData GetActionUpdateData(NotificationMessage notificationMessage)
        {
            ActionUpdateData actionUpdateData;
            if (notificationMessage != null)
            {
                var actionType = GetActionTypeFromMessageType(notificationMessage);
                var actionStatus = GetActionStatusFromMessageStatus(notificationMessage);
                if (notificationMessage.Data is CurrentPositionMessageData)
                {
                    actionUpdateData = new ActionUpdateData(NotificationType.CurrentActionStatus, actionType, actionStatus, (notificationMessage.Data as CurrentPositionMessageData)?.CurrentPosition);
                }
                else
                {
                    actionUpdateData = new ActionUpdateData(NotificationType.CurrentActionStatus, actionType, actionStatus);
                }
            }
            else
            {
                throw new ArgumentNullException();
            }
            return actionUpdateData;
        }

        private static ActionStatus GetActionStatusFromMessageStatus(NotificationMessage message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    return ActionStatus.Start;

                case MessageStatus.OperationExecuting:
                    return ActionStatus.Executing;

                case MessageStatus.OperationEnd:
                    return ActionStatus.Completed;

                case MessageStatus.OperationError:
                    return ActionStatus.Error;

                case MessageStatus.NoStatus:
                    return ActionStatus.None;

                default:
                    return ActionStatus.None;
            }
        }

        private static ActionType GetActionTypeFromMessageType(NotificationMessage message)
        {
            switch (message.Type)
            {
                case MessageType.CalibrateAxis:
                    if (message.Data is IHomingMessageData calibrateMessageData)
                    {
                        if (calibrateMessageData.AxisToCalibrate == Axis.Horizontal)
                        {
                            return ActionType.HorizontalHoming;
                        }

                        if (calibrateMessageData.AxisToCalibrate == Axis.Vertical)
                        {
                            return ActionType.VerticalHoming;
                        }
                    }

                    return ActionType.None;

                case MessageType.SwitchAxis:
                    return ActionType.SwitchEngine;

                case MessageType.Homing:
                    return ActionType.Homing;

                default:
                    return ActionType.None;
            }
        }

        #endregion
    }
}
