namespace Ferretto.VW.MAS_Utils.Enumerations
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
    }
}
