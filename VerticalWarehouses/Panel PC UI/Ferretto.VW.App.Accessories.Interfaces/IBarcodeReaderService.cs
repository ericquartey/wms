using System.Collections.ObjectModel;

namespace Ferretto.VW.App.Accessories.Interfaces
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
