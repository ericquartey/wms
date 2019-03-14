using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.MAStoUIMessages
{
    public class ActionUpdateData : IActionUpdateData
    {
        #region Constructors

        public ActionUpdateData(NotificationType notificationType, ActionType actionType, ActionStatus actionStatus)
        {
            this.NotificationType = notificationType;
            this.ActionType = actionType;
            this.ActionStatus = actionStatus;
        }

        #endregion

        #region Properties

        public ActionStatus ActionStatus { get; set; }

        public ActionType ActionType { get; set; }

        public NotificationType NotificationType { get; set; }

        #endregion
    }
}
