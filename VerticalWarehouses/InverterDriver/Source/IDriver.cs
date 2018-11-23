namespace Ferretto.VW.InverterDriver
{
   
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
