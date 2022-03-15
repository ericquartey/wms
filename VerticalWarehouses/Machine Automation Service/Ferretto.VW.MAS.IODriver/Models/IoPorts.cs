namespace Ferretto.VW.MAS.IODriver
{
    internal enum IoPorts
    {
        #region Outputs

        ResetSecurity = 0,

        ElevatorMotor = 1,

        CradleMotor = 2,

        MeasureProfile = 3,

        BayLight = 4,

        PowerEnable = 5,

        EndMissionRobot = 6,

        ReadyWarehouseRobot = 7,

        #endregion

        #region Inputs

        NormalState = 0,

        MushroomEmergency = 1,

        MicroCarterLeftSideBay = 2,

        MicroCarterRightSideBay = 3,

        AntiIntrusionBarrierBay = 4,

        LoadingUnitInBay = 5,

        LoadingUnitInLowerBay = 6,

        InverterInFault = 7,

        ElevatorMotorFeedback = 8,

        CradleMotorFeedback = 9,

        DrawerInMachineSide = 10,

        DrawerInOperatorSide = 11,

        CalibrationBarrierLight = 12,

        AntiIntrusionBarrier2Bay = 13,

        HookTrolley = 14,

        FinePickingRobot = 15,

        #endregion
    }
}
