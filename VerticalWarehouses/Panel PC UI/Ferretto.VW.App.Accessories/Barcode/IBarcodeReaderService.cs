using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.Devices;

namespace Ferretto.VW.App.Accessories
{
    public interface IBarcodeReaderService : IAccessoryService
    {
        #region Properties

        /// <summary>
        /// Gets the names of the active serial ports on the local machine.
        /// </summary>
        ObservableCollection<string> PortNames { get; }

        #endregion
    }
}
