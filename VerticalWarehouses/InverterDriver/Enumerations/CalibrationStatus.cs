namespace Ferretto.VW.Drivers.Inverter
{
    public enum CalibrationStatus
    {
        CALIBRATION_COMPLEATED = 00,

        #region Calibration Errors

        NO_ERROR = 10,

        INVALID_OPERATION = 11,

        INVALID_ARGUMENTS = 12,

        OPERATION_FAILED = 13,

        UNKNOWN_OPERATION = 14,

        #endregion

        #region Inverter Driver Error Status

        INVERTER_DRIVER_NO_ERROR = 20,

        INVERTER_DRIVER_HARDWARE_ERROR = 21,

        INVERTER_DRIVER_IO_ERROR = 22,

        INVERTER_DRIVER_INTERNAL_ERROR = 23,

        INVERTER_DRIVER_UNKNOWN_ERROR = 24,

        #endregion
    }
}
