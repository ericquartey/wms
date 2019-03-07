using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.InstallationApp.Interfaces;
using Ferretto.VW.InstallationApp.Resources.Enumerables;

namespace Ferretto.VW.InstallationApp.Resources
{
    public class MAS_EventMessage
    {
        #region Constructors

        public MAS_EventMessage(NotificationType notificationType, ActionType actionType, ActionStatus actionStatus, INotificationMessageData data)
        {
            this.ActionType = actionType;
            this.ActionStatus = actionStatus;
            this.NotificationType = notificationType;
            this.Data = data;
        }

        #endregion

        #region Properties

        public ActionStatus ActionStatus { get; set; }

        public ActionType ActionType { get; set; }

        public INotificationMessageData Data { get; set; }

        public NotificationType NotificationType { get; set; }

        #endregion
    }
}
