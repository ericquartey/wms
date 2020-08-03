namespace Ferretto.VW.Devices.TokenReader
{
    public sealed class TokenStatusChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public TokenStatusChangedEventArgs(bool isInserted, string serialNumber)
        {
            this.IsInserted = isInserted;
            this.SerialNumber = serialNumber;
        }

        #endregion

        #region Properties

        public bool IsInserted { get; }

        public string SerialNumber { get; }

        #endregion
    }
}
