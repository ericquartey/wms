using System;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Services
{
    internal class StatusMessageService : IStatusMessageService
    {
        #region Events

        public event EventHandler<StatusMessageEventArgs> StatusMessageNotified;

        #endregion

        #region Methods

        public void Clear()
        {
            this.StatusMessageNotified?.Invoke(this, new StatusMessageEventArgs(string.Empty, StatusMessageLevel.Info));
        }

        public void Notify(string message, StatusMessageLevel level = StatusMessageLevel.Info)
        {
            this.StatusMessageNotified?.Invoke(this, new StatusMessageEventArgs(message, level));
        }

        public void Notify(Exception exception, string detailedMessage = null)
        {
            this.Notify(exception?.Message + Environment.NewLine + detailedMessage, StatusMessageLevel.Error);
        }

        #endregion
    }
}
