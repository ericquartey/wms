using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Interfaces;

namespace Ferretto.VW.MAS_AutomationService
{
    public static class MessageParser
    {
        #region Methods

        public static ActionUpdateData GetActionUpdateData(NotificationMessage notificationMessage)
        {
            if (notificationMessage != null)
            {
                var actionType = GetActionTypeFromMessageType(notificationMessage.Type);
                var actionStatus = GetActionStatusFromMessageStatus(notificationMessage.Status);
                return new ActionUpdateData(NotificationType.CurrentActionStatus, actionType, actionStatus);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        private static ActionStatus GetActionStatusFromMessageStatus(MessageStatus messageStatus)
        {
            switch (messageStatus)
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

        private static ActionType GetActionTypeFromMessageType(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Homing:
                    return ActionType.Homing;

                case MessageType.HorizontalHoming:
                    return ActionType.HorizontalHoming;

                case MessageType.VerticalHoming:
                    return ActionType.VerticalHoming;

                case MessageType.SwitchAxis:
                    return ActionType.SwitchEngine;

                default:
                    return ActionType.None;
            }
        }

        #endregion
    }
}
