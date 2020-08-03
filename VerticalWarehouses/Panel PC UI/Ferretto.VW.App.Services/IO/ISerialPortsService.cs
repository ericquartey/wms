using System.Collections.ObjectModel;

namespace Ferretto.VW.App.Services
{
    public interface ISerialPortsService
    {
        #region Properties

        /// <summary>
        /// Gets the names of the active serial ports on the local machine.
        /// </summary>
        ObservableCollection<string> PortNames { get; }

        #endregion

        #region Methods

        void Start();

        void Stop();

        #endregion
    }
}
