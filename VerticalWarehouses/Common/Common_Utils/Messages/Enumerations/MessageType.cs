namespace Ferretto.VW.Common_Utils.Messages.Enumerations
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

        UpDownRepetitive,

        Positioning,

        CurrentEncoderPosition,

        ShutterPositioning,

        BeltBurnishing,
    }
}
