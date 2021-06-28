namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    // the Messages Data class name must begin with the MessageType name and end with MessageData
    // see NotificationMessageUiFactory.FromNotificationMessage
    public enum MessageType
    {
        NotSpecified,

        Homing,

        Stop,

        Movement,

        SensorsChanged,

        MachineMode,

        DataLayerReady,

        AssignedMissionChanged,

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

        InverterProgramming,

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

        ProfileCalibration,

        MoveTest,

        StopTest,

        WmsEnableChanged,

        SocketLinkEnableChanged,

        SocketLinkAlphaNumericBarChange,

        SocketLinkLaserPointerChange,

        /// <summary>
        /// Repetitive horizontal movements on a specified bay.
        /// </summary>
        RepetitiveHorizontalMovements,

        /// <summary>
        /// Low level driver generates this message to have a convenient slow clock for cleanup activities
        /// </summary>
        TimePeriodElapsed,

        /// <summary>
        /// Combined movements to load/unload a loading unit in elevator for 1 Ton machine.
        /// </summary>
        CombinedMovements,

        /// <summary>
        /// use light curtain to check intrusion
        /// </summary>
        CheckIntrusion,

        InverterReading,

        InverterParameters,

        /// <summary>
        /// Servicing and maintenance status to Telemetry
        /// </summary>
        ServicingSchedule,
    }
}
