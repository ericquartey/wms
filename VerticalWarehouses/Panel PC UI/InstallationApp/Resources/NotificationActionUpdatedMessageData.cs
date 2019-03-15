using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages;
using Ferretto.VW.InstallationApp.Interfaces;

namespace Ferretto.VW.InstallationApp.Resources
{
    public class NotificationActionUpdatedMessageData : INotificationActionUpdatedMessageData
    {
        #region Constructors

        public NotificationActionUpdatedMessageData(ActionUpdateData data)
        {
        }

        #endregion
    }
}
