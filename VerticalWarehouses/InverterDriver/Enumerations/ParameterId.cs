namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Parameter ID codes. The parametera are used in the Inverter driver.
    /// </summary>
    public enum ParameterId
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
}
