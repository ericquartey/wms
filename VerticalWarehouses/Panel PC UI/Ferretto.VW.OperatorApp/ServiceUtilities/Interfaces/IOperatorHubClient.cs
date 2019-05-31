using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces
{
    public interface IOperatorHubClient
    {
        #region Events

        event EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion

        #region Methods

        Task ConnectAsync();

        #endregion
    }
}
