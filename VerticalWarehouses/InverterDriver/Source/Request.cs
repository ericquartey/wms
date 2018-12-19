namespace Ferretto.VW.InverterDriver
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

    /// <summary>
    /// the Source of request
    /// </summary>
    public enum RequestSource
    {
        Internal,
        External
    }

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

        /// <summary>
        /// int data type (4 bytes)
        /// </summary>
        Int32,

        /// <summary>
        /// string data type (array of bytes)
        /// </summary>
        String
    }

    /// <summary>
    /// This class contains the definition  a request for the inverter.
    /// There are 2 type of request:
    ///  - the send request,
    ///  - the setting request.
    /// See Bonfiglioli inverter documentation for details about the telegram
    /// </summary>
    public class Request
    {
        // The request class contains description fields:
        //  - ParameterID
        //  - The source of request (internal or external)
        //  - Type of request (send request, or setting request)
        //  - The system index
        //  - The dataSet index
        //  - Value of parameter (available in different types - int, short, float, byte, string)

        #region Constructors

        public Request(TypeOfRequest t, ParameterID paramID, RequestSource source, byte systemIndex, byte dataSetIndex, ValueDataType dataType, object value)
        {
            this.Type = t;
            this.ParameterID = paramID;
            this.Source = source;
            this.SystemIndex = systemIndex;
            this.DataSetIndex = dataSetIndex;
            this.DataType = dataType;
            this.Value = value;
        }

        #endregion Constructors

        #region Properties

        public byte DataSetIndex { private set; get; }

        public ValueDataType DataType { private set; get; }

        public ParameterID ParameterID { get; set; }

        public byte SystemIndex { get; private set; }

        public TypeOfRequest Type { get; private set; }

        public object Value { get; set; }

        public RequestSource Source { get; private set; }

        #endregion Properties
    }
}
