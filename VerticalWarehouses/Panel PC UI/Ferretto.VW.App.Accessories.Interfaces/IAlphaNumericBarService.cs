using System.Threading.Tasks;
using Ferretto.VW.Devices;

namespace Ferretto.VW.App.Accessories.Interfaces
{
    public interface IAlphaNumericBarService
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

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        ///
        /// </summary>
        void StopAsync();

        #endregion
    }
}
