using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.Devices;

namespace Ferretto.VW.App.Accessories
{
    public interface IBarcodeReaderService
    {
        #region Properties

        /// <summary>
        /// Queries the device for information.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">An exception is thrown if the device does not support information querying.</exception>
        DeviceInformation DeviceInformation { get; }

        /// <summary>
        /// Gets the names of the active serial ports on the local machine.
        /// </summary>
        ObservableCollection<string> PortNames { get; }

        #endregion

        #region Methods

        void Disable();

        Task StartAsync();

        #endregion
    }
}
