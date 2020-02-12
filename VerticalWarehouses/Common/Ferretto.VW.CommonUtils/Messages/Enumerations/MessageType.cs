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

        /// <summary>
        /// A new WMS mission is available.
        /// </summary>
        /// <remarks>
        /// The <see cref="Ferretto.VW.MAS.MissionManager.WmsMissionProxyService"/> reacts to this event by querying the WMS for the new missions.
        /// </remarks>
        NewWmsMissionAvailable,

        /// <summary>
        /// A new machine mission is available.
        /// </summary>
        /// <remarks>
        /// The <see cref="Ferretto.VW.MAS.MissionManager.MissionSchedulingService"/> reacts to this event by checking the missions DB table.
        /// </remarks>
        NewMachineMissionAvailable,

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

        BayLight,

        FullTest,

        ProfileCalibration,
    }
}
