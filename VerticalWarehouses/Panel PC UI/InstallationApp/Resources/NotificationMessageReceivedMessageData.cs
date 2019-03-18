using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.InstallationApp.Interfaces;

namespace Ferretto.VW.InstallationApp.Resources
{
    public class NotificationMessageReceivedMessageData : INotificationMessageReceivedMessageData
    {
        #region Constructors

        public NotificationMessageReceivedMessageData(string s)
        {
            this.Message = s;
        }

        #endregion

        #region Properties

        public string Message { get; set; }

        #endregion
    }
}
