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

    /// <summary>
    /// Operation ID codes.
    /// The operation ID codes represents all operations performed by the VertiMag machine.
    /// </summary>
    public enum OperationID
    {
        /// <summary>
        /// Set Vertical axis origin.
        /// </summary>
        SetVerticalAxisOrigin = 0x00,

        /// <summary>
        /// Move along vertical axis to point.
        /// </summary>
        MoveAlongVerticalAxisToPoint = 0x01,

        /// <summary>
        /// Select type of motor movement (horizontal/vertical).
        /// </summary>
        SetTypeOfMotorMovement = 0x02,

        /// <summary>
        /// Move with given profile along horizontal axis.
        /// </summary>
        MoveAlongHorizontalAxisWithProfile = 0x03,

        /// <summary>
        /// Run the bay shutter (open/close).
        /// </summary>
        RunShutter = 0x04,

        /// <summary>
        /// Run weight detection routine.
        /// </summary>
        RunDrawerWeightRoutine = 0x05,

        /// <summary>
        /// Get drawer weight.
        /// </summary>
        GetDrawerWeight = 0x06,

        /// <summary>
        /// Stop.
        /// </summary>
        Stop = 0x07,

        /// <summary>
        /// Get main state of inverter.
        /// </summary>
        GetMainState = 0x08,

        /// <summary>
        /// Get IO sensors state.
        /// </summary>
        GetIOState = 0x09,

        /// <summary>
        /// Get IO emergency sensors state.
        /// </summary>
        GetIOEmergencyState = 0x0A,

        /// <summary>
        /// Set custom command.
        /// </summary>
        Set = 0x0B,

        /// <summary>
        /// None.
        /// </summary>
        None = 0xFF
    }
}
