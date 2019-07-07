namespace Ferretto.VW.Drivers.Inverter
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
}
