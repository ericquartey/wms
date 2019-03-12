using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.VWApp.Interfaces
{
    public interface INotificationCatcher
    {
        #region Methods

        void SubscribeInstallationMethodsToMAService();

        #endregion
    }
}
