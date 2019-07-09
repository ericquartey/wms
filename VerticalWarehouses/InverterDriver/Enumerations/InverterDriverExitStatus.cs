namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Inverter Driver exit status.
    /// </summary>
    public enum InverterDriverExitStatus
    {
        /// <summary>
        /// Successful operation
        /// </summary>
        Success = 0x0,

        /// <summary>
        /// Invalid argument
        /// </summary>
        InvalidArgument,

        /// <summary>
        /// Invalid operation
        /// </summary>
        InvalidOperation,

        /// <summary>
        /// Generic failure: see Errors enum
        /// </summary>
        Failure = 0xFF
    }
}
