using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.Interfaces
{
    public interface INotificationMessageData
    {
        #region Properties

        decimal? CurrentPosition { get; set; }

        #endregion
    }
}
