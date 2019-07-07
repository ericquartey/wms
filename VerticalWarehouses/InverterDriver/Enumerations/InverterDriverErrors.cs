namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Inverter Driver errors.
    /// </summary>
    public enum InverterDriverErrors
    {
        /// <summary>
        /// No error: no error encountered
        /// </summary>
        NoError = 0x00,

        /// <summary>
        /// Hardware error: not recovery condition
        /// </summary>
        HardwareError,

        /// <summary>
        /// IO error: communication error
        /// </summary>
        IOError,

        /// <summary>
        /// Internal error: software errors
        /// </summary>
        InternalError,

        /// <summary>
        /// Generic error: generic error
        /// </summary>
        GenericError = 0xFF
    }
}
