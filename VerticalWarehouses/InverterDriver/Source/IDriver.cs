namespace Ferretto.VW.InverterDriver
{
    /*
    /// <summary>
    /// Interface ICommandBase.
    /// </summary>
    public interface ICommandBase
    {
        /// <summary>
        /// Gets the Command Id.
        CommandId CmdId { get; }
    }
    */

    /// <summary>
    /// Interface IDriver.
    /// </summary>
    public interface IDriver
    {
        #region Properties

        /// <summary>
        /// <c>True</c> if last request has been executed.
        /// </summary>
        bool GetLastRequestDone { get; }

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
        /// Get value of given parameter.
        /// </summary>
        InverterDriverExitStatus EnquiryTelegram(ParameterID paramID, out object value);

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
        InverterDriverExitStatus GetIOState(int index, out bool retValue);

        /// <summary>
        /// Initialize the driver.
        /// </summary>
        /// <returns></returns>
        bool Initialize();

        /// <summary>
        /// Get value of given parameter. It is a echo for SettingRequest.
        /// </summary>
        InverterDriverExitStatus SelectTelegram(ParameterID paramID, out object value);

        /// <summary>
        /// Send a request to inverter to read parameter value.
        /// </summary>
        /// <returns></returns>
        InverterDriverExitStatus SendRequest(ParameterID paramID, byte systemIndex, byte dataSetIndex);

        /// <summary>
        /// Send a request to inverter to read a parameter value.
        /// </summary>
        InverterDriverExitStatus SettingRequest(ParameterID paramID, byte systemIndex, byte dataSetIndex, object value);

        /// <summary>
        /// Terminate and release driver' resources.
        /// </summary>
        void Terminate();

        #endregion Methods
    }
}
