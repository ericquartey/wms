using System.Collections.Generic;

namespace Ferretto.VW.MAS.IODriver.Interface.Services
{
    public interface IIoDeviceService
    {
        #region Properties

        IEnumerable<IoStatus> IoStatuses { get; }

        #endregion
    }
}
