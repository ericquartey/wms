namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Interface definition for [LastRequestDone] event arguments.
    /// </summary>
    public interface ILastRequestDoneEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the last request has been done.
        /// </summary>
        bool State { get; }

        #endregion
    }
}
