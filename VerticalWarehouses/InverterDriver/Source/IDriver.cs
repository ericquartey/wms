namespace Ferretto.VW.InverterDriver
{

    /// <summary>
    /// Interface IDriver.
    /// </summary>
    public interface IDriver
    {
        
        //! Set vertical axis origin routine.
        InverterDriverExitStatus SetVerticalAxisOrigin(byte direction, float vSearch, float vCam0, float a, float a1, float a2);

        //! Move along vertical axis to given point.
        InverterDriverExitStatus MoveAlongVerticalAxisToPoint(short x, float vMax, float a, float a1, float w);

        //! Select movement among vertical movement and horizontal movement.
        InverterDriverExitStatus SelectMovement(byte m);

        //! Move along horizontal axis with given profile.
        InverterDriverExitStatus MoveAlongHorizontalAxisWithProfile(float v1, float a, short s1, short s2, float v2, float a1, short s3, short s4, float v3, float a2, short s5, short s6, float a3, short s7);

        //! Run shutter on opening movement or closing movement.
        InverterDriverExitStatus RunShutter(byte m);

        //! Run routine for detect the weight of current drawer.
        InverterDriverExitStatus RunDrawerWeightRoutine(short d, float w, float a, byte e);

        //! Get the drawer weight
        InverterDriverExitStatus GetDrawerWeight(out float ic);

        //! Stop.
        InverterDriverExitStatus Stop();

        //! Set ON/OFF value to the given line.
        InverterDriverExitStatus Set(int i, byte value);

        //! Get main status.
        byte GetMainState { get; }

        //! Get IO sensor state.
        int[] GetIOState { get; }

        //! Get IO emergency sensors state.
        byte[] GetIOEmergencyState { get; }

        //! Set/Get IP address to connect
        string IPAddressToConnect { set; get; }

        //! Set/Get port address to connect
        int PortAddressToConnect { set; get; }

    } // interface IDriver


    /// <summary>
    /// Interface ICommandBase.
    /// </summary>
    public interface ICommandBase
    {
        /// <summary>
        /// Gets the Command Id.
        CommandId CmdId { get; }

    } // interface ICommandBase


}
