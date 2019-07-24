namespace Ferretto.VW.CommonUtils.Enumerations
{
    public enum IOMachineSensors
    {
        #region INFO Elevator Sensors

        EmergencyEndRun = 0,

        ZeroVertical = 1,

        ElevatorMotorSelected = 2,

        CradleMotorSelected = 3,

        #endregion

        #region INFO Cradle Sensors

        ZeroPawl = 4,

        LuPresentInMachineSide = 5,

        LuPresentInOperatorSide = 6,

        #endregion

        #region INFO Bays & HeightControl Sensors

        LUPresentInBay1 = 7,

        HeightControlCheckBay1 = 8,

        ShutterSensorABay1 = 9,

        ShutterSensorBBay1 = 10,

        LUPresentInBay2 = 11,

        HeightControlCheckBay2 = 12,

        ShutterSensorABay2 = 13,

        ShutterSensorBBay2 = 14,

        LUPresentInBay3 = 15,

        HeightControlCheckBay3 = 16,

        ShutterSensorABay3 = 17,

        ShutterSensorBBay3 = 18,

        #endregion

        #region INFO Various Inputs

        NormalState = 19,

        MushroomHeadButtonBay1 = 20,

        MicroCarterLeftSideBay1 = 21,

        MicroCarterRightSideBay1 = 22,

        AntiIntrusionShutterBay1 = 23,

        MushroomHeadButtonBay2 = 24,

        MicroCarterLeftSideBay2 = 25,

        MicroCarterRightSideBay2 = 26,

        AntiIntrusionShutterBay2 = 27,

        MushroomHeadButtonBay3 = 28,

        MicroCarterLeftSideBay3 = 29,

        MicroCarterRightSideBay3 = 30,

        AntiIntrusionShutterBay3 = 31,

        #endregion
    }
}
