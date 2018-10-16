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
        Read = 0x0,

        /// <summary>
        /// Request for writing a parameter value
        /// </summary>
        Write
    }

    /// <summary>
    /// This class contains the parameter of a request for the inverter.
    /// There are 2 type of request:
    ///  - the read parameter request (a.k.a. SendRequest),
    ///  - the write parameter request (a.k.a. PositioningRequest)
    /// See Bonfiglioli inverter documentation for details about the telegram
    /// </summary>
    public class Request
    {
        // The request class contains description fields:
        //  - ParameterID
        //  - Type of request (send request, or positioning request)
        //  - value of parameter (available in different types - int, short, float, byte, string)

        #region Fields

        public const short HOMING_CREEP_SPEED_PARAM = 1133;
        public const short HOMING_FAST_SPEED_PARAM = 1132;
        public const short HOMING_MODE_PARAM = 1130;

        public const short HOMING_OFFSET_PARAM = 1131;
        public const short POSITION_ACCELERATION_PARAM = 1457;

        public const short POSITION_DECELERATION_PARAM = 1458;

        public const short POSITION_TARGET_POSITION_PARAM = 1455;
        public const short POSITION_TARGET_SPEED_PARAM = 1456;
        public const short SET_OPERATING_MODE_PARAM = 1454;

        private int parameterId;
        private TypeOfRequest typeOfRequest;
        private byte valueParameterByte;
        private float valueParameterFloat;
        private short valueParameterInt16;
        private int valueParameterInt32;
        private string valueParameterString;

        #endregion Fields

        #region Constructors

        public Request(TypeOfRequest t, int parameterId)
        {
            this.typeOfRequest = t;
            this.parameterId = parameterId;
        }

        #endregion Constructors

        #region Properties

        public int ParameterID
        {
            get => this.parameterId;
            set => this.parameterId = value;
        }

        public byte ParameterValueByte
        {
            get => this.valueParameterByte;
            set => this.valueParameterByte = value;
        }

        public float ParameterValueFloat
        {
            get => this.ParameterValueFloat;
            set => this.valueParameterFloat = value;
        }

        public short ParameterValueInt16
        {
            get => this.valueParameterInt16;
            set => this.valueParameterInt16 = value;
        }

        public int ParameterValueInt32
        {
            get => this.valueParameterInt32;
            set => this.valueParameterInt32 = value;
        }

        public string ParameterValueString
        {
            get => this.valueParameterString;
            set => this.valueParameterString = value;
        }

        public TypeOfRequest Type
        {
            get => this.typeOfRequest;
            set => this.typeOfRequest = value;
        }

        #endregion Properties
    }
}
