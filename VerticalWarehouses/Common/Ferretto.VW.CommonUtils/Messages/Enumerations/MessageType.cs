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

        MachineStateActive,

        MachineStatusActive,

        NewMissionAvailable,

        CreateMission,

        Positioning,

        CurrentPosition,

        ShutterPositioning,

        FsmException,

        InverterException,

        DlException,

        ResolutionCalibration,

        IoDriverException,

        MissionOperationCompleted,

        MissionManagerInitialized,

        BayOperationalStatusChanged,

        WebApiException,

        CheckCondition,

        NewMissionOperationAvailable,

        ErrorStatusChanged,

        InverterStop,

        PowerEnable,

        InverterStatusWord,

        RunningStateChanged,

        FaultStateChanged,

        ElevatorWeightCheck,

        PositioningTable,

        InverterFaultReset,

        ResetSecurity,

        WeightAcquisitionCommand,

        MissionManagerException,

        ChangeRunningState,

        InverterPowerEnable
    }
}
