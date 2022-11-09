using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IAccessoriesDataProvider
    {
        #region Methods

        public bool CheckAccessories();

        BayAccessories GetAccessories(BayNumber bayNumber);
        AlphaNumericBar GetAlphaNumericBar(BayNumber bayNumber);
        LaserPointer GetLaserPointer(BayNumber bayNumber);

        void UpdateAlphaNumericBar(BayNumber bayNumber, bool isEnabled, string ipAddress, int port, AlphaNumericBarSize size, int maxMessageLength, bool clearOnClose, bool? useGet = false, List<string> messageFields = null);

        void UpdateBarcodeReaderDeviceInfo(BayNumber bayNumber, DeviceInformation deviceInformation);

        void UpdateBarcodeReaderSettings(BayNumber bayNumber, bool isEnabled, string portName);

        void UpdateCardReaderSettings(BayNumber bayNumber, bool isEnabled, string tokenRegex, bool isLocal);

        void UpdateLabelPrinterSettings(BayNumber bayNumber, bool isEnabled, string printerName);

        void UpdateLaserPointer(BayNumber bayNumber, bool isEnabled, string ipAddress, int port, double xOffset, double yOffset, double zOffsetLowerPosition, double zOffsetUpperPosition);

        void UpdateTokenReaderSettings(BayNumber bayNumber, bool isEnabled, string portName);

        void UpdateWeightingScaleDeviceInfo(BayNumber bayNumber, DeviceInformation deviceInformation);

        void UpdateWeightingScaleSettings(BayNumber bayNumber, bool isEnabled, string ipAddress, int port, WeightingScaleModelNumber modelNumber);

        #endregion
    }
}
