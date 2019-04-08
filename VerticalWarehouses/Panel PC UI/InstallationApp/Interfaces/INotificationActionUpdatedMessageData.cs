using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.Interfaces
{
    public interface INotificationActionUpdatedMessageData : INotificationMessageData
    {
        #region Properties

        decimal? CurrentEncoderPosition { get; set; }

        int? CurrentShutterPosition { get; set; }

        #endregion
    }
}
