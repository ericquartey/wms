using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.Devices.BarcodeReader;

namespace Ferretto.VW.App.Accessories.Interfaces
{
    public interface IBarcodeReaderService : IAccessoryService
    {
        #region Properties

        DeviceModel DeviceModel { get; set; }

        void SimulateRead(string barcode);

        #endregion

        #region Methods

        Task UpdateSettingsAsync(bool isEnabled, string portName, DeviceModel deviceModel);

        #endregion
    }
}
