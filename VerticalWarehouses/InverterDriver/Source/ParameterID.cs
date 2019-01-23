namespace Ferretto.VW.InverterDriver
{
    /// <summary>
    /// Parameter ID codes. The parameter are used in the Inverter driver.
    /// Constants enumerative
    /// </summary>
    public enum ParameterID
    {
        CONTROL_WORD_PARAM = 410,
        HOMING_CREEP_SPEED_PARAM = 1133,
        HOMING_FAST_SPEED_PARAM = 1132,
        HOMING_MODE_PARAM = 1130,
        HOMING_OFFSET_PARAM = 1131,
        HOMING_ACCELERATION = 1134,
        POSITION_ACCELERATION_PARAM = 1457,
        POSITION_DECELERATION_PARAM = 1458,
        POSITION_TARGET_POSITION_PARAM = 1455,
        POSITION_TARGET_SPEED_PARAM = 1456,
        SET_OPERATING_MODE_PARAM = 1454,
        STATUS_WORD_PARAM = 411,
        ACTUAL_POSITION_SHAFT = 1108,
        STATUS_DIGITAL_SIGNALS = 250,
        CONTROL_MODE_PARAM = 412,   // Local/Remote: 1-State Machine
        ANALOG_IC_PARAM = 457
    }

    /// <summary>
    /// The parameter IDs class for the inverter.
    /// This class contains the list of parameter IDs
    /// </summary>
    public class ParameterIDClass
    {
        #region Fields

        private static readonly ParameterIDClass instance = new ParameterIDClass();

        #endregion Fields

        #region Properties

        public static ParameterIDClass Instance => instance;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Get the value data type {byte, float, Int16, ...} related to the given parameter ID.
        /// </summary>
        public ValueDataType GetDataValueType(ParameterID p)
        {
            var type = ValueDataType.Int16;

            switch (p)
            {
                case ParameterID.CONTROL_WORD_PARAM: type = ValueDataType.UInt16; break;
                case ParameterID.HOMING_CREEP_SPEED_PARAM: type = ValueDataType.Int32; break;
                case ParameterID.HOMING_FAST_SPEED_PARAM: type = ValueDataType.Int32; break;
                case ParameterID.HOMING_MODE_PARAM: type = ValueDataType.Int16; break;
                case ParameterID.HOMING_OFFSET_PARAM: type = ValueDataType.Int16; break;
                case ParameterID.HOMING_ACCELERATION: type = ValueDataType.Int32; break;
                case ParameterID.POSITION_ACCELERATION_PARAM: type = ValueDataType.Float; break;
                case ParameterID.POSITION_DECELERATION_PARAM: type = ValueDataType.Float; break;
                case ParameterID.POSITION_TARGET_POSITION_PARAM: type = ValueDataType.Int32; break;
                case ParameterID.POSITION_TARGET_SPEED_PARAM: type = ValueDataType.Float; break;
                case ParameterID.SET_OPERATING_MODE_PARAM: type = ValueDataType.Int16; break;
                case ParameterID.STATUS_WORD_PARAM: type = ValueDataType.UInt16; break;
                case ParameterID.STATUS_DIGITAL_SIGNALS: type = ValueDataType.Int16; break;
                case ParameterID.ACTUAL_POSITION_SHAFT: type = ValueDataType.Int32; break;
                case ParameterID.CONTROL_MODE_PARAM: type = ValueDataType.UInt16; break;
                case ParameterID.ANALOG_IC_PARAM: type = ValueDataType.Int16; break;
            }

            return type;
        }

        /// <summary>
        /// Retrieve the parameterID code for a given decimal value.
        /// </summary>
        public ParameterID ValueToParameterIDCode(short value)
        {
            var paramID = ParameterID.STATUS_WORD_PARAM;

            switch (value)
            {
                case 410: paramID = ParameterID.CONTROL_WORD_PARAM; break;
                case 1133: paramID = ParameterID.HOMING_CREEP_SPEED_PARAM; break;
                case 1132: paramID = ParameterID.HOMING_FAST_SPEED_PARAM; break;
                case 1130: paramID = ParameterID.HOMING_MODE_PARAM; break;
                case 1131: paramID = ParameterID.HOMING_OFFSET_PARAM; break;
                case 1457: paramID = ParameterID.POSITION_ACCELERATION_PARAM; break;
                case 1458: paramID = ParameterID.POSITION_DECELERATION_PARAM; break;
                case 1455: paramID = ParameterID.POSITION_TARGET_POSITION_PARAM; break;
                case 1456: paramID = ParameterID.POSITION_TARGET_SPEED_PARAM; break;
                case 1454: paramID = ParameterID.SET_OPERATING_MODE_PARAM; break;
                case 411: paramID = ParameterID.STATUS_WORD_PARAM; break;
                case 250: paramID = ParameterID.STATUS_DIGITAL_SIGNALS; break;
                case 1108: paramID = ParameterID.ACTUAL_POSITION_SHAFT; break;
                case 412: paramID = ParameterID.CONTROL_MODE_PARAM; break;
                case 457: paramID = ParameterID.ANALOG_IC_PARAM; break;
            }

            return paramID;
        }

        #endregion Methods
    }
}
