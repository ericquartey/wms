namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Represents a request for the Inverter.
    /// </summary>
    /// <remarks>
    /// See Bonfiglioli inverter documentation for details about the telegram.
    /// </remarks>
    public class Request
    {
        #region Constructors

        public Request(
            TypeOfRequest typeOfRequest,
            ParameterId parameterId,
            RequestSource source,
            byte systemIndex,
            byte dataSetIndex,
            ValueDataType dataType,
            object value)
        {
            this.Type = typeOfRequest;
            this.ParameterId = parameterId;
            this.Source = source;
            this.SystemIndex = systemIndex;
            this.DataSetIndex = dataSetIndex;
            this.DataType = dataType;
            this.Value = value;
        }

        #endregion

        #region Properties

        public byte DataSetIndex { get; private set; }

        public ValueDataType DataType { get; private set; }

        public ParameterId ParameterId { get; set; }

        public RequestSource Source { get; private set; }

        public byte SystemIndex { get; private set; }

        public TypeOfRequest Type { get; private set; }

        public object Value { get; set; }

        #endregion
    }
}
