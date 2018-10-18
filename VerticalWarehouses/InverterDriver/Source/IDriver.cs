namespace Ferretto.VW.InverterDriver
{
    /// <summary>
    /// Interface ICommandBase.
    /// </summary>
    public interface ICommandBase
    {
        #region Properties

        /// <summary>
        /// Gets the Command Id.
        CommandId CmdId { get; }

        #endregion Properties
    }

    /// <summary>
    /// Interface IDriver.
    /// </summary>
    public interface IDriver
    {
        #region Properties

        /// <summary>
        /// Get main status.
        /// </summary>
        /// <returns></returns>
        InverterDriverState GetMainState { get; }

        /// <summary>
        ///  Set/Get IP address to connect
        /// </summary>
        string IPAddressToConnect { set; get; }

        /// <summary>
        /// Get last error.
        /// </summary>
        InverterDriverErrors LastError { get; }

        /// <summary>
        /// Set/Get port address to connect
        /// </summary>
        int PortAddressToConnect { set; get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Get the drawer weight
        /// </summary>
        /// <param name="ic"></param>
        /// <returns></returns>
        InverterDriverExitStatus GetDrawerWeight(float ic);

        /// <summary>
        /// Get IO emergency sensors state.
        /// </summary>
        /// <returns></returns>
        InverterDriverExitStatus GetIOEmergencyState();

        /// <summary>
        /// Get IO sensor state.
        /// </summary>
        /// <returns></returns>
        InverterDriverExitStatus GetIOState();

        /// <summary>
        /// Move along horizontal axis with given profile.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="a"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="v2"></param>
        /// <param name="a1"></param>
        /// <param name="s3"></param>
        /// <param name="s4"></param>
        /// <param name="v3"></param>
        /// <param name="a2"></param>
        /// <param name="s5"></param>
        /// <param name="s6"></param>
        /// <param name="a3"></param>
        /// <param name="s7"></param>
        /// <returns></returns>
        InverterDriverExitStatus MoveAlongHorizontalAxisWithProfile(float v1, float a, short s1, short s2, float v2, float a1, short s3, short s4, float v3, float a2, short s5, short s6, float a3, short s7);

        /// <summary>
        /// Move along vertical axis to given point.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="vMax"></param>
        /// <param name="acc"></param>
        /// <param name="dec"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        InverterDriverExitStatus MoveAlongVerticalAxisToPoint(short x, float vMax, float acc, float dec, float w);

        /// <summary>
        /// Run routine for detect the weight of current drawer.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="w"></param>
        /// <param name="a"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        InverterDriverExitStatus RunDrawerWeightRoutine(short d, float w, float a, byte e);

        /// <summary>
        /// Run shutter on opening movement or closing movement.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        InverterDriverExitStatus RunShutter(byte m);

        /// <summary>
        /// Set ON/OFF value to the given line.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        InverterDriverExitStatus Set(int i, byte value);

        /// <summary>
        /// Set type of motor movement between vertical or horizontal.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        InverterDriverExitStatus SetTypeOfMotorMovement(byte m);

        /// <summary>
        /// Set vertical axis origin routine.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="vSearch"></param>
        /// <param name="vCam0"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        InverterDriverExitStatus SetVerticalAxisOrigin(byte mode, float vSearch, float vCam0, float offset);

        /// <summary>
        /// Stop.
        /// </summary>
        /// <returns></returns>
        InverterDriverExitStatus Stop();

        #endregion Methods
    }
}
