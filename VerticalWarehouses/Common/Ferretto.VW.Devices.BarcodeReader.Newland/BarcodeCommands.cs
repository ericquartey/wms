namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    public static class BarcodeCommands
    {
        #region Fields

        public const string AutoMode = "99900111";

        public const string DecodeSessionTimeout = "99900150";

        public const string DisableAutoPowerOff = "99900175";

        public const string EnterProgrammingMode = "$$$$";

        public const string EnterSetup = "99900031";

        public const string ExitProgrammingMode = "%%%%";

        public const string ExitSetup = "99900031";

        public const string InquireAllInformation = "99900300";

        public const string InquireFirmwareVersion = "99900301";

        public const string InquireManufactureDate = "99900303";

        public const string InquireModelNumber = "99900304";

        public const string InquireSerialNumber = "99900302";

        public const string ManualMode = "99900110";

        public const string PowerOff = "99900100";

        public const string PowerOffTimeout10Minutes = "99900171";

        public const string PowerOffTimeout20Minutes = "99900172";

        public const string PowerOffTimeout30Minutes = "99900173";

        public const string PowerOffTimeout5Minutes = "99900170";

        public const string PowerOffTimeout60Minutes = "99900174";

        public const string RebootCradle = "99900105";

        public const string RebootScanner = "99900104";

        public const string RereadSameBarcodeWithADelay = "99900201";

        public const string RereadSameBarcodeWithNoDelay = "99900200";

        public const string ResetCradle = "99900044";

        public const string ResetScanner = "99900030";

        public const string TestMode = "99900103";

        public const string TimeoutBetweenDecodesSameBarcode = "99900167";

        public const string UndoPairing = "99900045";

        #endregion
    }
}
