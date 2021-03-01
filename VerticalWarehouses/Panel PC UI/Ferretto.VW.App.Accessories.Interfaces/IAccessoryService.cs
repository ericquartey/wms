using System.Threading;
using System.Threading.Tasks;

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
        Ferretto.VW.Devices.DeviceInformation DeviceInformation { get; }

        #endregion

        #region Methods

        Task StartAsync();

        Task StopAsync();

        #endregion
    }
}
