namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// The parameter IDs class for the inverter.
    /// This class contains the list of parameter IDs
    /// </summary>
    public static class ParameterIdClass
    {
        #region Methods

        /// <summary>
        /// Get the value data type related to the given parameter ID.
        /// </summary>
        public static ValueDataType GetDataValueType(ParameterId parameterId)
        {
            var type = ValueDataType.Int16;

            switch (parameterId)
            {
                case ParameterId.CONTROL_WORD_PARAM:
                    type = ValueDataType.UInt16;
                    break;

                case ParameterId.HOMING_CREEP_SPEED_PARAM:
                    type = ValueDataType.Int32;
                    break;

                case ParameterId.HOMING_FAST_SPEED_PARAM:
                    type = ValueDataType.Int32;
                    break;

                case ParameterId.HOMING_MODE_PARAM:
                    type = ValueDataType.Int16;
                    break;

                case ParameterId.HOMING_OFFSET_PARAM:
                    type = ValueDataType.Int16;
                    break;

                case ParameterId.HOMING_ACCELERATION:
                    type = ValueDataType.Int32;
                    break;

                case ParameterId.POSITION_ACCELERATION_PARAM:
                    type = ValueDataType.Float;
                    break;

                case ParameterId.POSITION_DECELERATION_PARAM:
                    type = ValueDataType.Float;
                    break;

                case ParameterId.POSITION_TARGET_POSITION_PARAM:
                    type = ValueDataType.Int32;
                    break;

                case ParameterId.POSITION_TARGET_SPEED_PARAM:
                    type = ValueDataType.Int32;
                    break;

                case ParameterId.SET_OPERATING_MODE_PARAM:
                    type = ValueDataType.Int16;
                    break;

                case ParameterId.STATUS_WORD_PARAM:
                    type = ValueDataType.UInt16;
                    break;

                case ParameterId.STATUS_DIGITAL_SIGNALS:
                    type = ValueDataType.Int16;
                    break;

                case ParameterId.ACTUAL_POSITION_SHAFT:
                    type = ValueDataType.Int32;
                    break;

                case ParameterId.CONTROL_MODE_PARAM:
                    type = ValueDataType.UInt16;
                    break;

                case ParameterId.ANALOG_IC_PARAM:
                    type = ValueDataType.Int16;
                    break;
            }

            return type;
        }

        /// <summary>
        /// Retrieve the ParameterId code for a given decimal value.
        /// </summary>
        public static ParameterId ValueToParameterIdCode(short value)
        {
            var paramId = ParameterId.STATUS_WORD_PARAM;

            switch (value)
            {
                case 410:
                    paramId = ParameterId.CONTROL_WORD_PARAM;
                    break;

                case 1133:
                    paramId = ParameterId.HOMING_CREEP_SPEED_PARAM;
                    break;

                case 1132:
                    paramId = ParameterId.HOMING_FAST_SPEED_PARAM;
                    break;

                case 1130:
                    paramId = ParameterId.HOMING_MODE_PARAM;
                    break;

                case 1131:
                    paramId = ParameterId.HOMING_OFFSET_PARAM;
                    break;

                case 1134:
                    paramId = ParameterId.HOMING_ACCELERATION;
                    break;

                case 1457:
                    paramId = ParameterId.POSITION_ACCELERATION_PARAM;
                    break;

                case 1458:
                    paramId = ParameterId.POSITION_DECELERATION_PARAM;
                    break;

                case 1455:
                    paramId = ParameterId.POSITION_TARGET_POSITION_PARAM;
                    break;

                case 1456:
                    paramId = ParameterId.POSITION_TARGET_SPEED_PARAM;
                    break;

                case 1454:
                    paramId = ParameterId.SET_OPERATING_MODE_PARAM;
                    break;

                case 411:
                    paramId = ParameterId.STATUS_WORD_PARAM;
                    break;

                case 250:
                    paramId = ParameterId.STATUS_DIGITAL_SIGNALS;
                    break;

                case 1108:
                    paramId = ParameterId.ACTUAL_POSITION_SHAFT;
                    break;

                case 412:
                    paramId = ParameterId.CONTROL_MODE_PARAM;
                    break;

                case 457:
                    paramId = ParameterId.ANALOG_IC_PARAM;
                    break;
            }

            return paramId;
        }

        #endregion
    }
}
