namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Interface definition for [Error] event arguments.
    /// </summary>
    /// TODO: To be removed.
    public interface IErrorEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the error code.
        /// </summary>
        InverterDriverErrors ErrorCode { get; }

        #endregion
    }
}
