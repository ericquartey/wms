namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    public enum BarcodeCommand
    {
        #region Fields

        AutoMode = 99900111,

        DecodeSessionTimeout = 99900150,

        DisableAutoPowerOff = 99900175,

        EnterSetup = 99900031,

        InquireAllInformation = 99900300,

        InquireFirmwareVersion = 99900301,

        InquireManufactureDate = 99900303,

        InquireModelNumber = 99900304,

        InquireSerialNumber = 99900302,

        ManualMode = 99900110,

        PowerOff = 99900100,

        PowerOffTimeout10Minutes = 99900171,

        PowerOffTimeout20Minutes = 99900172,

        PowerOffTimeout30Minutes = 99900173,

        PowerOffTimeout5Minutes = 99900170,

        PowerOffTimeout60Minutes = 99900174,

        RebootCradle = 99900105,

        RebootScanner = 99900104,

        RereadSameBarcodeWithADelay = 99900201,

        RereadSameBarcodeWithNoDelay = 99900200,

        ResetCradle = 99900044,

        ResetScanner = 99900030,

        TestMode = 99900103,

        TimeoutBetweenDecodesSameBarcode = 99900167,

        UndoPairing = 99900045,

        #endregion
    }
}
