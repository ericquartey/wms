namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    internal class CommandString
    {
        #region Constructors

        public CommandString(string command)
        {
            this.Command = command;
        }

        #endregion

        #region Properties

        public string Command { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"#{this.Command};";
        }

        #endregion
    }
}
