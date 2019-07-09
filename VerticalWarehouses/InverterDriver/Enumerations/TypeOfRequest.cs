namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Type of request for the inverter.
    /// </summary>
    public enum TypeOfRequest
    {
        /// <summary>
        /// Request for reading a parameter
        /// </summary>
        SendRequest = 0x0,

        /// <summary>
        /// Request for writing a parameter value
        /// </summary>
        SettingRequest
    }
}
