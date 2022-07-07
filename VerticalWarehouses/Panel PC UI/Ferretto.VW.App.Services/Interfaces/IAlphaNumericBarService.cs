using System.Threading.Tasks;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.Devices;

namespace Ferretto.VW.App.Services
{
    public interface IAlphaNumericBarService : IAccessoryService
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

        Task AlphaNumericBarConfigureAsync();
        Task ClearMessage();

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        ///
        /// </summary>
        Task StopAsync();

        #endregion
    }
}
