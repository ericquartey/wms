using System.Collections.Generic;

namespace Ferretto.VW.MAS.IODriver.Interface.Services
{
    public interface IIoDevicesProvider
    {
        #region Properties

        IEnumerable<IoStatus> Devices { get; }

        #endregion
    }
}
