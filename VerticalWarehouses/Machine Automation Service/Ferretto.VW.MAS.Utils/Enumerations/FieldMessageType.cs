namespace Ferretto.VW.MAS.Utils.Enumerations
{
    public enum FieldMessageType
    {
        NoType = 0,

        SwitchAxis,

        IoReset,

        DataLayerReady,

        IoPowerUp,

        MeasureProfile,

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

        InverterDisable,

        InverterProgramming,

        AxisPosition,

        Positioning,

        ShutterPositioning,

        SensorsChanged,

        IoDriverException,

        SetConfigurationIo,

        ResetSecurity,

        PowerEnable,

        InverterFaultReset,

        InverterStatusWord,

        InverterSetTimer,

        ContinueMovement,

        InverterCurrentError,

        LaserOn,

        LaserOff,

        LaserMove,

        LaserMoveAndSwitchOn,

        BayLight,

        InverterReading
    }
}
