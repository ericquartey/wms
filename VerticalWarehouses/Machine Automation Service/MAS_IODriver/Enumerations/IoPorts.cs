namespace Ferretto.VW.MAS_IODriver.Enumerations
{
    public enum IoPorts
    {
        #region Outputs

        ResetSecurity = 0,

        ElevatorMotor = 1,

        CradleMotor = 2,

        MeasureBarrier = 3,

        BayLight = 4,

        EndMissionRobot = 6,

        ReadyWarehouseRobot = 7,

        #endregion

        #region Inputs

        NormalState = 0,

        MushroomEmergency = 1,

        MicroCarterLeftSideBay = 2,

        MicroCarterRightSideBay = 3,

        AntiIntrusionShutterBay = 4,

        LoadingUnitExistenceInBay = 5,

        DrawerInBaySecondaryPosition = 6,

        ElevatorMotorFeedback = 8,

        CradleMotorFeedback = 9,

        DrawerInMachineSide = 10,

        DrawerInOperatorSide = 11,

        HeightControlCheckBay = 12,

        HookTrolley = 14,

        FinePickingRobot = 15

        #endregion
    }
}
