using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.Interfaces
{
    public interface INotificationMessageSensorsChangedData : INotificationMessageData
    {
        #region Properties

        bool[] SensorsStates { get; set; }

        #endregion
    }
}
