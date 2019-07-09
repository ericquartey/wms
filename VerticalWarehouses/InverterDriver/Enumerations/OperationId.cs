namespace Ferretto.VW.Drivers.Inverter
{
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
