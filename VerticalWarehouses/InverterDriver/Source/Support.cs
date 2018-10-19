// -----------------------------------------------------------------------------------------------------
// This class contains the definition of enumeratives, structures, constants used by the Inverter driver.

namespace Ferretto.VW.InverterDriver
{
    /// <summary>
    /// Status of inverter machine.
    /// </summary>
    public enum HardwareInverterStatus
    {
        /// <summary>
        /// Not operative (OFF status)
        /// </summary>
        NotOperative = 0x0,

        /// <summary>
        /// Operative (ON status)
        /// </summary>
        Operative = 0x1
    }

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

    /// <summary>
    /// Inverter Driver exist status.
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
