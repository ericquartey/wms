namespace Ferretto.VW.MAS.Utils.Enumerations
{
    public enum FieldMessageType
    {
        NoType = 0,

        SwitchAxis,

        IoReset,

        DataLayerReady,

        IoPowerUp,

        CalibrateAxis,

        InverterPowerOff,

        InverterPowerOn,

        InverterStatusUpdate,

        InverterOperationTimeout,

        InverterStop,

        InverterSwitchOn,

        InverterSwitchOff,

        InverterException,

        InverterError,

        AxisPosition,

        Positioning,

        ShutterPositioning,

        SensorsChanged,

        IoDriverException,

        SetConfigurationIO,

        ResetSecurity,

        PowerEnable
    }
}
