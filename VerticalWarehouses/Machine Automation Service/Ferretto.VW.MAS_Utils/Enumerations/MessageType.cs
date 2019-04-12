namespace Ferretto.VW.MAS_Utils.Enumerations
{
    public enum MessageType
    {
        NoType,

        Homing,

        Stop,

        Movement,

        SensorsChanged,

        DataLayerReady,

        SwitchAxis,

        CalibrateAxis,

        ShutterControl,

        AddMission,

        CreateMission,

        BeltBreakIn,

        Positioning,

        CurrentEncoderPosition,

        ShutterPositioning
    }
}
