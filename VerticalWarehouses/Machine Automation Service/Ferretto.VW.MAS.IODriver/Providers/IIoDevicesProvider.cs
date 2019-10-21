using System.Collections.Generic;

namespace Ferretto.VW.MAS.IODriver
{
    public interface IIoDevicesProvider
    {
        #region Properties

        IEnumerable<IoStatus> Devices { get; }

        #endregion
    }
}
