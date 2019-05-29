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

        MissionAdded,

        CreateMission,

        UpDownRepetitive,

        Positioning,

        CurrentEncoderPosition,

        ShutterPositioning,

        VerticalPositioning,

        FSMException,

        InverterException,

        DLException,

        ResolutionCalibration,

        IoDriverException,

        MissionCompleted,

        MissionManagerInitialized,
    }
}
