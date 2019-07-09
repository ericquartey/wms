﻿using System.Collections;

namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Interface IDriver.
    /// </summary>
    public interface IDriver
    {
        #region Properties

        /// <summary>
        /// Get brake resistance overtemperature-Digital value.
        /// </summary>
        bool Brake_Resistance_Overtemperature { get; }

        /// <summary>
        /// Get the current position of controlled Shaft (horizontal axis).
        /// </summary>
        int Current_Position_Horizontal_Shaft { get; }

        /// <summary>
        /// Get the current position of controlled Shaft (vertical axis).
        /// </summary>
        int Current_Position_Vertical_Shaft { get; }

        /// <summary>
        /// Get Emergency Stop-Digital value.
        /// </summary>
        bool Emergency_Stop { get; }

        /// <summary>
        /// Enable the automatic updating of current position shaft parameter (horizontal axis).
        /// </summary>
        bool Enable_Update_Current_Position_Horizontal_Shaft_Mode { get; set; }

        /// <summary>
        /// Enable the automatic updating of current position shaft parameter (vertical axis).
        /// </summary>
        bool Enable_Update_Current_Position_Vertical_Shaft_Mode { get; set; }

        /// <summary>
        /// Enable the retrieve of StatusWord parameter value.
        /// </summary>
        bool Get_Status_Word_Enable { get; set; }

        /// <summary>
        /// Gets the main status.
        /// </summary>
        /// <returns></returns>
        InverterDriverState GetMainState { get; }

        /// <summary>
        ///  Set/Get IP address to connect.
        /// </summary>
        string IPAddressToConnect { get; set; }

        /// <summary>
        /// Gets last error.
        /// </summary>
        InverterDriverErrors LastError { get; }

        /// <summary>
        /// Get Pawl Sensor Zero-Digital value.
        /// </summary>
        bool Pawl_Sensor_Zero { get; }

        /// <summary>
        /// Set/Get port address to connect.
        /// </summary>
        int PortAddressToConnect { get; set; }

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

        #endregion

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
        InverterDriverExitStatus SendRequest(ParameterId parameterId, byte systemIndex, byte dataSetIndex);

        /// <summary>
        /// Send a request to inverter to read a parameter value.
        /// </summary>
        InverterDriverExitStatus SettingRequest(ParameterId parameterId, byte systemIndex, byte dataSetIndex, object value);

        /// <summary>
        /// Terminate and release driver' resources.
        /// </summary>
        void Terminate();

        #endregion
    }
}
