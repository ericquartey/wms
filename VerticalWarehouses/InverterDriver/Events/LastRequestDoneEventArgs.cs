namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// [LastRequestDone] event arguments.
    /// </summary>
    public class LastRequestDoneEventArgs : System.EventArgs, ILastRequestDoneEventArgs
    {
        #region Constructors

        public LastRequestDoneEventArgs(bool state)
        {
            this.State = state;
        }

        #endregion

        #region Properties

        public bool State { get; }

        #endregion
    }
}
