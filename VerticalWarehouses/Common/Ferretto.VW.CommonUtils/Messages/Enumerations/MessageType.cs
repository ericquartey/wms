namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    public enum MessageType
    {
        NotSpecified,

        Homing,

        Stop,

        Movement,

        SensorsChanged,

        DataLayerReady,

        ExecuteMission,

        SwitchAxis,

        CalibrateAxis,

        MachineStateActive,

        MachineStatusActive,

        MachineMode,

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

        InverterFaultReset,

        ResetSecurity,

        WeightAcquisitionCommand,

        MachineManagerException,

        MissionManagerException,

        ChangeRunningState,

        InverterPowerEnable,

        MoveLoadingUnit,

        ContinueMovement,
    }
}
