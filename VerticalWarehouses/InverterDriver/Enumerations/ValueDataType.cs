namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Data type of the payload parameter for a request.
    /// </summary>
    public enum ValueDataType
    {
        /// <summary>
        /// byte data type (1 byte)
        /// </summary>
        Byte = 0x0,

        /// <summary>
        /// float data type (4 bytes)
        /// </summary>
        Float,

        /// <summary>
        /// double data type (8 bytes)
        /// </summary>
        Double,

        /// <summary>
        /// short data type (2 bytes)
        /// </summary>
        Int16,

        UInt16,

        /// <summary>
        /// int data type (4 bytes)
        /// </summary>
        Int32,

        /// <summary>
        /// string data type (array of bytes)
        /// </summary>
        String
    }
}
