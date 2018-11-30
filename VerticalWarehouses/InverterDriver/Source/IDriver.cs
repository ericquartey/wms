using System.Collections;

namespace Ferretto.VW.InverterDriver
{
   
    /// <summary>
    /// Interface IDriver.
    /// </summary>
    public interface IDriver
    {
        #region Properties

        /// <summary>
        /// BitArray for StatusWord
        /// </summary>
        BitArray Status_Word { get; }

        bool Get_Status_Word_Enable { get; set; }

        bool Get_Actual_Position_Shaft_Enable { get; set; }

        int Actual_Position_Shaft { get; }

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

        /// <summary>
        /// Get brake resistance overtemperature-Digital value
        /// </summary>
        bool Brake_Resistance_Overtemperature { get; }

        /// <summary>
        /// Get Emergency Stop-Digital value
        /// </summary>
        bool Emergency_Stop { get; }

        /// <summary>
        /// Get Pawl Sensor Zero-Digital value
        /// </summary>
        bool Pawl_Sensor_Zero { get; }

        /// <summary>
        /// Get Udc Presence Cradle Operator-Digital value
        /// </summary>
        bool Udc_Presence_Cradle_Operator { get; }

        /// <summary>
        /// Get Udc Presence Cradle Machine-Digital value
        /// </summary>
        bool Udc_Presence_Cradle_Machine { get; }

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
