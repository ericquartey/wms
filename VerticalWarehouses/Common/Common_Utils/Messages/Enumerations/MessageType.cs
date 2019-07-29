namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    public enum MessageType
    {
        NoType,

        Homing,

        Stop,

        Movement,

        SensorsChanged,

        DataLayerReady,

        DrawerOperation,

        ExecuteMission,

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

        BayConnected,

        WebApiException,

        CheckCondition,

        ResetHardware
    }
}
