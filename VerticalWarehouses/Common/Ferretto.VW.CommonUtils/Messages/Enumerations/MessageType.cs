namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    public enum MessageType
    {
        NotSpecified,

        Homing,

        Stop,

        Movement,

        SensorsChanged,

        MachineMode,

        DataLayerReady,

        AssignedMissionOperationChanged,

        SwitchAxis,

        CalibrateAxis,

        MachineStateActive,

        MachineStatusActive,

        NewMissionAvailable,

        CreateMission,

        Positioning,

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

        ElevatorPosition,

        BayChainPosition,
    }
}
