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

        NewMissionAvailable,

        CreateMission,

        Positioning,

        CurrentEncoderPosition,

        ShutterPositioning,

        FSMException,

        InverterException,

        DLException,

        ResolutionCalibration,

        IoDriverException,

        MissionOperationCompleted,

        MissionManagerInitialized,

        BayOperationalStatusChanged,

        WebApiException,

        CheckCondition,

        NewMissionOperationAvailable,

        ResetSecurity,

        InverterStop,

    }
}
