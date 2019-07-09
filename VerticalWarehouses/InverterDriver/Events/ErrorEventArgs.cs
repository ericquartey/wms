namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// [Error] event arguments.
    /// </summary>
    public class ErrorEventArgs : System.EventArgs, IErrorEventArgs
    {
        #region Constructors

        public ErrorEventArgs(InverterDriverErrors error)
        {
            this.ErrorCode = error;
        }

        #endregion

        #region Properties

        public InverterDriverErrors ErrorCode { get; }

        #endregion
    }
}
