using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Devices;

namespace Ferretto.VW.App.Accessories
{
    public interface IAccessoryService
    {
        #region Properties

        /// <summary>
        /// Queries the device for information.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">An exception is thrown if the device does not support information querying.</exception>
        DeviceInformation DeviceInformation { get; }

        #endregion

        #region Methods

        Task StartAsync();

        Task StopAsync();

        #endregion
    }
}
