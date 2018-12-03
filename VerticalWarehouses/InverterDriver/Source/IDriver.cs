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
        /// Get the Actual Position of controlled Shaft.
        /// </summary>
        int Actual_Position_Shaft { get; }

        /// <summary>
        /// Get brake resistance overtemperature-Digital value.
        /// </summary>
        bool Brake_Resistance_Overtemperature { get; }

        /// <summary>
        /// Get Emergency Stop-Digital value.
        /// </summary>
        bool Emergency_Stop { get; }

        /// <summary>
        /// Enable the retrieve of Actual_Position_Shaft parameter.
        /// </summary>
        bool Get_Actual_Position_Shaft_Enable { get; set; }

        /// <summary>
        /// Enable the retrieve of StatusWord parameter value.
        /// </summary>
        bool Get_Status_Word_Enable { get; set; }

        /// <summary>
        /// Get main status.
        /// </summary>
        /// <returns></returns>
        InverterDriverState GetMainState { get; }

        /// <summary>
        ///  Set/Get IP address to connect.
        /// </summary>
        string IPAddressToConnect { set; get; }

        /// <summary>
        /// Get last error.
        /// </summary>
        InverterDriverErrors LastError { get; }

        /// <summary>
        /// Get Pawl Sensor Zero-Digital value.
        /// </summary>
        bool Pawl_Sensor_Zero { get; }

        /// <summary>
        /// Set/Get port address to connect.
        /// </summary>
        int PortAddressToConnect { set; get; }

        /// <summary>
        /// Get the BitArray for StatusWord parameter value.
        /// </summary>
        BitArray Status_Word { get; }

        /// <summary>
        /// Get Udc Presence Cradle Machine-Digital value.
        /// </summary>
        bool Udc_Presence_Cradle_Machine { get; }

        /// <summary>
        /// Get Udc Presence Cradle Operator-Digital value.
        /// </summary>
        bool Udc_Presence_Cradle_Operator { get; }

        #endregion Properties

        #region Methods

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
