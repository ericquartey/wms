namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// [Connected] event arguments.
    /// </summary>
    public class ConnectedEventArgs : System.EventArgs, IConnectedEventArgs
    {
        #region Constructors

        public ConnectedEventArgs(bool state)
        {
            this.State = state;
        }

        #endregion

        #region Properties

        public bool State { get; }

        #endregion
    }
}
