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

        Positioning,

        CurrentEncoderPosition,

        ShutterPositioning,

        FSMException,

        InverterException,

        DLException,

        ResolutionCalibration,

        IoDriverException,

        MissionCompleted,

        MissionManagerInitialized,
    }
}
