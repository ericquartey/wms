namespace Ferretto.VW.MAS_Utils.Enumerations
{
    public enum FieldMessageType
    {
        NoType = 0,

        SwitchAxis,

        IoReset,

        DataLayerReady,

        IoPowerUp,

        Stop,

        CalibrateAxis,

        InverterReset,

        InverterOperationTimeout
    }
}
