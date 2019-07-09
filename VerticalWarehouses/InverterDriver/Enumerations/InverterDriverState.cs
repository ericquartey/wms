namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Inverter states.
    /// </summary>
    public enum InverterDriverState
    {
        /// <summary>
        /// Idle: not connected
        /// </summary>
        Idle,

        /// <summary>
        /// Ready: initialized and ready to operate
        /// </summary>
        Ready,

        /// <summary>
        /// Working: perform an operation
        /// </summary>
        Working,

        /// <summary>
        /// Error: the Inverter occurs in an irreversible error state
        /// </summary>
        Error
    }
}
